import { AfterViewInit, Directive, ElementRef, OnDestroy, inject, input } from '@angular/core';
import { ScrollTrigger, gsap, reducedMotion, registerGsap } from './motion';

/** Fades/slides an element in as it scrolls into view. */
@Directive({ selector: '[revealOnScroll]' })
export class RevealOnScrollDirective implements AfterViewInit, OnDestroy {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);
  readonly delay = input(0, { alias: 'revealOnScrollDelay' });

  private tween?: gsap.core.Tween;

  ngAfterViewInit(): void {
    registerGsap();
    if (reducedMotion()) return;

    this.tween = gsap.from(this.el.nativeElement, {
      y: 26,
      opacity: 0,
      duration: 0.7,
      ease: 'power2.out',
      delay: this.delay(),
      scrollTrigger: { trigger: this.el.nativeElement, start: 'top 90%', once: true },
    });
  }

  ngOnDestroy(): void {
    this.tween?.scrollTrigger?.kill();
    this.tween?.kill();
  }
}
