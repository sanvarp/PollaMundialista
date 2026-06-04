import { gsap } from 'gsap';
import { Flip } from 'gsap/Flip';
import { ScrollTrigger } from 'gsap/ScrollTrigger';
import { SplitText } from 'gsap/SplitText';

let registered = false;

/** Registers GSAP plugins once (all free since GSAP 3.13). */
export function registerGsap(): void {
  if (registered) return;
  gsap.registerPlugin(ScrollTrigger, Flip, SplitText);
  registered = true;
}

/** True when the user asked for reduced motion — animations should degrade. */
export function reducedMotion(): boolean {
  return typeof matchMedia !== 'undefined' && matchMedia('(prefers-reduced-motion: reduce)').matches;
}

export { Flip, gsap, ScrollTrigger, SplitText };
