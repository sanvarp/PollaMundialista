import { AfterViewInit, Component, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import Lenis from 'lenis';
import { ScrollTrigger, gsap, reducedMotion, registerGsap } from './core/motion/motion';
import { Cursor } from './shared/ui/cursor/cursor';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Cursor],
  template: `
    <router-outlet />
    <app-cursor />
  `,
  styles: [':host { display: block; min-height: 100dvh; }'],
})
export class App implements AfterViewInit, OnDestroy {
  private lenis?: Lenis;
  private ticker?: (time: number) => void;

  ngAfterViewInit(): void {
    registerGsap();
    if (reducedMotion()) return;

    // High-end smooth scrolling, kept in sync with GSAP ScrollTrigger.
    this.lenis = new Lenis({ duration: 1.1, smoothWheel: true });
    this.lenis.on('scroll', ScrollTrigger.update);

    this.ticker = (time: number) => this.lenis?.raf(time * 1000);
    gsap.ticker.add(this.ticker);
    gsap.ticker.lagSmoothing(0);
  }

  ngOnDestroy(): void {
    if (this.ticker) gsap.ticker.remove(this.ticker);
    this.lenis?.destroy();
  }
}
