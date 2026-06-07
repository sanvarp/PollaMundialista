import { AfterViewInit, Directive, ElementRef, OnDestroy, inject, input } from '@angular/core';
import { SplitText, gsap, reducedMotion, registerGsap } from './motion';

/**
 * Reveals a headline character-by-character with a staggered rise.
 * Waits for web fonts so the split measures the final glyphs.
 */
// Intentional terse attribute API for an app-level motion utility (no prefix).
// eslint-disable-next-line @angular-eslint/directive-selector
@Directive({ selector: '[splitReveal]' })
export class SplitRevealDirective implements AfterViewInit, OnDestroy {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);
  readonly delay = input(0, { alias: 'splitRevealDelay' });

  private split?: SplitText;

  ngAfterViewInit(): void {
    registerGsap();
    if (reducedMotion()) return;

    const run = () => {
      this.split = new SplitText(this.el.nativeElement, { type: 'chars,words' });
      gsap.set(this.el.nativeElement, { opacity: 1 });
      gsap.from(this.split.chars, {
        yPercent: 120,
        opacity: 0,
        ease: 'power3.out',
        duration: 0.8,
        stagger: 0.02,
        delay: this.delay(),
      });
    };

    const fonts = (document as Document & { fonts?: FontFaceSet }).fonts;
    if (fonts?.ready) fonts.ready.then(run);
    else run();
  }

  ngOnDestroy(): void {
    this.split?.revert();
  }
}
