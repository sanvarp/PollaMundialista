import { Directive, ElementRef, effect, inject, input } from '@angular/core';
import { gsap, reducedMotion, registerGsap } from './motion';

/**
 * Animates a number from its previous value to the bound target.
 * Use on an element with NO text interpolation — the directive writes textContent.
 *   <span [countUp]="points"></span>
 */
// Intentional terse attribute API for an app-level motion utility (no prefix).
// eslint-disable-next-line @angular-eslint/directive-selector
@Directive({ selector: '[countUp]' })
export class CountUpDirective {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);
  readonly value = input.required<number>({ alias: 'countUp' });

  private current = 0;

  constructor() {
    registerGsap();
    effect(() => {
      const target = this.value();
      if (reducedMotion()) {
        this.el.nativeElement.textContent = String(target);
        this.current = target;
        return;
      }
      const obj = { v: this.current };
      gsap.to(obj, {
        v: target,
        duration: 0.9,
        ease: 'power2.out',
        onUpdate: () => (this.el.nativeElement.textContent = String(Math.round(obj.v))),
        onComplete: () => (this.current = target),
      });
    });
  }
}
