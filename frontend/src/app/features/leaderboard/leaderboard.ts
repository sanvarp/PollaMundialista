import {
  Component,
  DestroyRef,
  ElementRef,
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

  private readonly board = viewChild<ElementRef<HTMLElement>>('board');
  /** In-flight reorder animation, killed on destroy so tweens don't outlive the view. */
  private currentFlip?: ReturnType<typeof Flip.from>;

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
      this.currentFlip?.kill();
    });
  }

  private load(initial: boolean): void {
    if (initial) this.loading.set(true);

    // Capture positions before the data changes, to animate the reorder.
    const container = this.board()?.nativeElement;
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
        this.entries.set(e);
        this.loading.set(false);
        if (flipState && changed && container) {
          // Wait for the DOM to reflect the new order, then animate from old positions.
          requestAnimationFrame(() =>
            requestAnimationFrame(() => {
              // Boundary-crossers we detect ourselves: elements present now but not before.
              const entering = [
                ...container.querySelectorAll<HTMLElement>('[data-flip-id]'),
              ].filter((el) => !oldIds.has(el.dataset['flipId'] ?? ''));
              // Pin the wrapper height so it doesn't collapse while rows are absolute.
              const h = container.getBoundingClientRect().height;
              gsap.set(container, { height: h });
              // Suppress the slots' CSS transitions so they don't fight Flip's
              // transform reset (otherwise the side podium slots wobble at the end).
              container.classList.add('is-flipping');
              this.currentFlip = Flip.from(flipState, {
                duration: 0.6,
                ease: 'power2.out',
                absolute: true,
                stagger: { each: 0.035, from: 'start' },
                // The card/row a crosser leaves behind cleanly disappears.
                onLeave: (els) =>
                  gsap.to(els, {
                    opacity: 0,
                    scale: 0.78,
                    duration: 0.28,
                    ease: 'power2.in',
                    overwrite: 'auto',
                  }),
                onComplete: () => {
                  gsap.set(container, { clearProps: 'height' });
                  requestAnimationFrame(() => container.classList.remove('is-flipping'));
                },
              });
              // ...then a crosser BUILDS into its new slot, delayed so it follows the exit
              // and the reflow: old disappears -> survivors slide -> new card/row grows in.
              if (entering.length) {
                gsap.fromTo(
                  entering,
                  { opacity: 0, scale: 0.8 },
                  {
                    opacity: 1,
                    scale: 1,
                    duration: 0.5,
                    delay: 0.3,
                    ease: 'back.out(1.5)',
                    overwrite: 'auto',
                    stagger: 0.06,
                  },
                );
              }
            }),
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
