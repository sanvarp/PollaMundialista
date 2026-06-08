import {
  Component,
  DestroyRef,
  ElementRef,
  Injector,
  afterNextRender,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { LeaderboardService } from '../../core/api/leaderboard.service';
import { AuthService } from '../../core/auth/auth.service';
import { CountUpDirective } from '../../core/motion/count-up.directive';
import { Flip, gsap, reducedMotion, registerGsap } from '../../core/motion/motion';
import { SplitRevealDirective } from '../../core/motion/split-reveal.directive';
import { LeaderboardEntry } from '../../core/models/leaderboard.models';
import { AppHeader } from '../../shared/ui/app-header/app-header';

const POLL_MS = 12_000;

@Component({
  selector: 'app-leaderboard',
  imports: [AppHeader, CountUpDirective, SplitRevealDirective],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.scss',
})
export class Leaderboard {
  private readonly api = inject(LeaderboardService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly injector = inject(Injector);

  private readonly board = viewChild<ElementRef<HTMLElement>>('board');
  /** In-flight reorder animation + its companion tweens, cleaned up on overlap/destroy. */
  private currentFlip?: ReturnType<typeof Flip.from>;
  private enterTween?: gsap.core.Tween;
  private ghostTween?: gsap.core.Tween;
  /** Pending post-render hook that mounts the next Flip; cancelled if a new load supersedes it. */
  private pendingRender?: ReturnType<typeof afterNextRender>;

  protected readonly entries = signal<LeaderboardEntry[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly myId = this.auth.user()?.userId ?? '';

  /** Top 3 reordered for a podium: 2nd, 1st, 3rd. */
  protected readonly podium = computed(() => {
    const t = this.entries().slice(0, 3);
    return [t[1], t[0], t[2]].filter(Boolean) as LeaderboardEntry[];
  });
  protected readonly rest = computed(() => this.entries().slice(3));

  /** The current user's own entry, for the "your position" summary. */
  protected readonly me = computed(
    () => this.entries().find((e) => e.userId === this.myId) ?? null,
  );

  constructor() {
    registerGsap();
    this.load(true);

    // Poll so a result posted by an admin animates in (Flip) without a manual refresh.
    const timer = setInterval(() => this.load(false), POLL_MS);
    this.destroyRef.onDestroy(() => {
      clearInterval(timer);
      this.resetAnimation();
    });
  }

  /**
   * Abort and fully clean up any in-flight reorder animation. Called before each new
   * animation (so an overlapping refresh/poll/admin-result can't leave rows stuck in
   * position:absolute or capture a dirty state) and on destroy. `progress(1)` makes Flip
   * run its own style-reversion before we kill it; the rest is belt-and-suspenders.
   */
  private resetAnimation(): void {
    // Cancel a post-render hook that hasn't fired yet (a superseding load), so two reorders
    // can't both mount a Flip in the same tick.
    this.pendingRender?.destroy();
    this.pendingRender = undefined;
    this.currentFlip?.progress(1).kill();
    this.enterTween?.kill();
    this.ghostTween?.kill();
    this.currentFlip = undefined;
    this.enterTween = undefined;
    this.ghostTween = undefined;
    document.querySelectorAll('.lb-ghost').forEach((g) => g.remove());
    const container = this.board()?.nativeElement;
    if (!container) return;
    container.classList.remove('is-flipping');
    const items = container.querySelectorAll('[data-flip-id]');
    gsap.killTweensOf(items);
    gsap.set(items, { clearProps: 'position,top,left,width,height,margin,transform,opacity' });
    container
      .querySelectorAll('.podium, .board')
      .forEach((el) => gsap.set(el, { clearProps: 'height' }));
  }

  private load(initial: boolean): void {
    if (initial) this.loading.set(true);

    // Capture positions before the data changes, to animate the reorder.
    const container = this.board()?.nativeElement;
    // Abort any animation still running (overlapping refresh/poll/admin result) so we
    // capture a clean state and never leave survivors pinned absolute.
    if (!initial && container) this.resetAnimation();
    const flipState =
      !initial && !reducedMotion() && container
        ? Flip.getState(container.querySelectorAll('[data-flip-id]'))
        : null;
    // IDs present BEFORE the change, so we can detect boundary-crossers ourselves —
    // Flip's own enter/leave detection is unreliable across our two containers.
    const oldIds = new Set<string>();
    if (flipState && container) {
      container
        .querySelectorAll<HTMLElement>('[data-flip-id]')
        .forEach((el) => oldIds.add(el.dataset['flipId'] ?? ''));
    }

    this.api.getLeaderboard().subscribe({
      next: (e) => {
        const changed =
          JSON.stringify(e.map((x) => x.userId)) !==
          JSON.stringify(this.entries().map((x) => x.userId));

        // Clone the elements about to LEAVE (a player crossing the podium/board boundary)
        // BEFORE Angular removes them, so we animate their disappearance ourselves. They go
        // on document.body (position:fixed) so Angular's change detection can't wipe them.
        const ghosts: HTMLElement[] = [];
        if (flipState && changed && container) {
          const newIds = new Set(e.map((x, i) => (i < 3 ? 'p-' : 'b-') + x.userId));
          container.querySelectorAll<HTMLElement>('[data-flip-id]').forEach((el) => {
            if (newIds.has(el.dataset['flipId'] ?? '')) return;
            const r = el.getBoundingClientRect();
            const g = el.cloneNode(true) as HTMLElement;
            g.removeAttribute('data-flip-id');
            g.classList.add('lb-ghost');
            Object.assign(g.style, {
              position: 'fixed',
              margin: '0',
              pointerEvents: 'none',
              zIndex: '40',
              top: `${r.top}px`,
              left: `${r.left}px`,
              width: `${r.width}px`,
              height: `${r.height}px`,
            });
            document.body.appendChild(g);
            ghosts.push(g);
          });
        }

        this.entries.set(e);
        this.loading.set(false);
        if (flipState && changed && container) {
          // Run after Angular renders the new order but BEFORE the browser paints it, so a
          // rising row is never shown at its destination for one frame before Flip animates it
          // from its old position (that flash was the "blink into place" before the swap).
          this.pendingRender = afterNextRender(
            () => {
              this.pendingRender = undefined;
              // Boundary-crossers we detect ourselves: elements present now but not before.
              const entering = [
                ...container.querySelectorAll<HTMLElement>('[data-flip-id]'),
              ].filter((el) => !oldIds.has(el.dataset['flipId'] ?? ''));
              // Capture each enterer's CORRECT final rect now, while everything is still in
              // flow. Once Flip makes the survivors absolute, an entering element would be
              // alone in its container and get mis-placed — so we pin it here instead.
              const cr = container.getBoundingClientRect();
              const enterRects = entering.map((el) => {
                const r = el.getBoundingClientRect();
                return { el, top: r.top - cr.top, left: r.left - cr.left, w: r.width, h: r.height };
              });
              // Pin the inner containers' heights so neither collapses while its items are
              // absolute — otherwise the board header jumps up into the podium's vacated space.
              const inner = [
                container.querySelector<HTMLElement>('.podium'),
                container.querySelector<HTMLElement>('.board'),
              ].filter((el): el is HTMLElement => el != null);
              inner.forEach((el) => gsap.set(el, { height: el.getBoundingClientRect().height }));
              // Suppress the slots' CSS transitions so they don't fight Flip's transform reset.
              container.classList.add('is-flipping');
              // Survivors glide to their new spots.
              this.currentFlip = Flip.from(flipState, {
                duration: 0.6,
                ease: 'power2.out',
                absolute: true,
                stagger: { each: 0.035, from: 'start' },
                onComplete: () => {
                  inner.forEach((el) => gsap.set(el, { clearProps: 'height' }));
                  enterRects.forEach(({ el }) =>
                    gsap.set(el, { clearProps: 'position,top,left,width,height,margin' }),
                  );
                  requestAnimationFrame(() => container.classList.remove('is-flipping'));
                },
              });
              // The card/row a crosser left behind cleanly disappears (its clone)...
              if (ghosts.length) {
                this.ghostTween = gsap.to(ghosts, {
                  opacity: 0,
                  scale: 0.85,
                  duration: 0.35,
                  ease: 'power2.in',
                  overwrite: 'auto',
                  onComplete: () => ghosts.forEach((g) => g.remove()),
                });
              }
              // ...while the new card/row — pinned at its correct spot so the absolute
              // survivors can't mis-place it — builds in (no delay -> no empty gap).
              enterRects.forEach(({ el, top, left, w, h }) =>
                gsap.set(el, { position: 'absolute', top, left, width: w, height: h, margin: 0 }),
              );
              if (entering.length) {
                this.enterTween = gsap.fromTo(
                  entering,
                  { opacity: 0, scale: 0.85 },
                  {
                    opacity: 1,
                    scale: 1,
                    duration: 0.5,
                    ease: 'back.out(1.4)',
                    overwrite: 'auto',
                    stagger: 0.05,
                  },
                );
              }
            },
            { injector: this.injector },
          );
        }
      },
      error: () => {
        this.error.set('No se pudo cargar la tabla.');
        this.loading.set(false);
      },
    });
  }

  refresh(): void {
    this.load(false);
  }

  openUser(userId: string): void {
    this.router.navigate(['/users', userId]);
  }
}
