import { AfterViewInit, Component, ElementRef, OnDestroy, viewChild } from '@angular/core';
import { gsap, reducedMotion } from '../../../core/motion/motion';

/**
 * Custom pointer: a precise dot plus a ring that trails with easing and grows
 * over interactive elements. Only enabled on fine-pointer, motion-OK devices.
 */
@Component({
  selector: 'app-cursor',
  template: `
    <div #dot class="cursor-dot"></div>
    <div #ring class="cursor-ring"></div>
  `,
  styleUrl: './cursor.scss',
})
export class Cursor implements AfterViewInit, OnDestroy {
  private readonly dot = viewChild.required<ElementRef<HTMLElement>>('dot');
  private readonly ring = viewChild.required<ElementRef<HTMLElement>>('ring');

  private onMove?: (e: PointerEvent) => void;
  private onOver?: (e: PointerEvent) => void;
  private enabled = false;

  ngAfterViewInit(): void {
    const fine = matchMedia('(hover: hover) and (pointer: fine)').matches;
    if (reducedMotion() || !fine) return;

    this.enabled = true;
    document.documentElement.classList.add('has-custom-cursor');

    const dot = this.dot().nativeElement;
    const ring = this.ring().nativeElement;

    const dotX = gsap.quickTo(dot, 'x', { duration: 0.08, ease: 'power3' });
    const dotY = gsap.quickTo(dot, 'y', { duration: 0.08, ease: 'power3' });
    const ringX = gsap.quickTo(ring, 'x', { duration: 0.32, ease: 'power3' });
    const ringY = gsap.quickTo(ring, 'y', { duration: 0.32, ease: 'power3' });

    this.onMove = (e) => {
      dotX(e.clientX);
      dotY(e.clientY);
      ringX(e.clientX);
      ringY(e.clientY);
    };

    this.onOver = (e) => {
      const interactive = (e.target as HTMLElement)?.closest('button, a, input, [role="button"]');
      gsap.to(ring, { scale: interactive ? 1.8 : 1, duration: 0.25, ease: 'power2.out' });
      gsap.to(ring, { borderColor: interactive ? 'var(--accent)' : 'rgba(154,160,171,0.6)', duration: 0.25 });
    };

    window.addEventListener('pointermove', this.onMove, { passive: true });
    window.addEventListener('pointerover', this.onOver, { passive: true });
  }

  ngOnDestroy(): void {
    if (this.onMove) window.removeEventListener('pointermove', this.onMove);
    if (this.onOver) window.removeEventListener('pointerover', this.onOver);
    if (this.enabled) document.documentElement.classList.remove('has-custom-cursor');
  }
}
