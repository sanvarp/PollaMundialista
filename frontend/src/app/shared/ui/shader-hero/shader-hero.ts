import { AfterViewInit, Component, ElementRef, OnDestroy, viewChild } from '@angular/core';
import * as THREE from 'three';
import { reducedMotion } from '../../../core/motion/motion';

const VERTEX = /* glsl */ `
  varying vec2 vUv;
  void main() {
    vUv = uv;
    gl_Position = vec4(position.xy, 0.0, 1.0);
  }
`;

// Slow, flowing mesh-gradient in the brand palette. Cheap: a couple of moving
// soft centers over a near-black base, with a vignette. It's a backdrop.
const FRAGMENT = /* glsl */ `
  precision highp float;
  uniform float u_time;
  varying vec2 vUv;

  void main() {
    vec2 uv = vUv;
    float t = u_time * 0.05;

    vec2 c1 = vec2(0.30 + 0.20 * sin(t * 1.1), 0.42 + 0.18 * cos(t * 0.9));
    vec2 c2 = vec2(0.72 + 0.18 * cos(t * 0.8), 0.60 + 0.20 * sin(t * 1.3));
    vec2 c3 = vec2(0.55 + 0.22 * sin(t * 0.6 + 1.5), 0.25 + 0.15 * cos(t * 1.0));

    float d1 = smoothstep(0.55, 0.0, distance(uv, c1));
    float d2 = smoothstep(0.70, 0.0, distance(uv, c2));
    float d3 = smoothstep(0.50, 0.0, distance(uv, c3));

    vec3 base = vec3(0.039, 0.043, 0.055);  // #0a0b0e
    vec3 deep = vec3(0.07, 0.09, 0.13);
    vec3 lime = vec3(0.84, 1.0, 0.247);      // #d6ff3f

    vec3 col = base;
    col = mix(col, deep, d2 * 0.6);
    col = mix(col, deep * 1.4, d3 * 0.4);
    col = mix(col, lime, d1 * 0.10);         // subtle lime glow only

    float vig = smoothstep(1.15, 0.25, distance(uv, vec2(0.5)));
    col *= mix(0.65, 1.0, vig);

    gl_FragColor = vec4(col, 1.0);
  }
`;

@Component({
  selector: 'app-shader-hero',
  template: `<canvas #canvas class="shader-canvas" [class.is-static]="staticFallback"></canvas>`,
  styleUrl: './shader-hero.scss',
})
export class ShaderHero implements AfterViewInit, OnDestroy {
  private readonly canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');

  protected staticFallback = false;

  private renderer?: THREE.WebGLRenderer;
  private scene?: THREE.Scene;
  private camera?: THREE.Camera;
  private material?: THREE.ShaderMaterial;
  private rafId = 0;
  private running = false;
  private observer?: IntersectionObserver;
  private onResize?: () => void;
  private onVisibility?: () => void;

  ngAfterViewInit(): void {
    const smallScreen = matchMedia('(max-width: 640px)').matches;
    if (reducedMotion() || smallScreen) {
      // Degrade to a CSS gradient (handled by .is-static); no WebGL loop.
      this.staticFallback = true;
      return;
    }
    this.init();
  }

  private init(): void {
    const canvas = this.canvas().nativeElement;
    try {
      this.renderer = new THREE.WebGLRenderer({
        canvas,
        antialias: false,
        alpha: false,
        powerPreference: 'low-power',
      });
    } catch {
      this.staticFallback = true;
      return;
    }
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2)); // cap DPR

    this.scene = new THREE.Scene();
    this.camera = new THREE.Camera();
    this.material = new THREE.ShaderMaterial({
      vertexShader: VERTEX,
      fragmentShader: FRAGMENT,
      uniforms: { u_time: { value: 0 } },
    });
    this.scene.add(new THREE.Mesh(new THREE.PlaneGeometry(2, 2), this.material));

    this.resize();
    this.onResize = () => this.resize();
    window.addEventListener('resize', this.onResize);

    // Pause when the tab is hidden.
    this.onVisibility = () => (document.hidden ? this.stop() : this.start());
    document.addEventListener('visibilitychange', this.onVisibility);

    // Pause when scrolled offscreen.
    this.observer = new IntersectionObserver(
      ([entry]) => (entry.isIntersecting && !document.hidden ? this.start() : this.stop()),
      { threshold: 0 },
    );
    this.observer.observe(canvas);

    this.start();
  }

  private resize(): void {
    if (!this.renderer) return;
    const canvas = this.canvas().nativeElement;
    const w = canvas.clientWidth || window.innerWidth;
    const h = canvas.clientHeight || window.innerHeight;
    this.renderer.setSize(w, h, false);
  }

  private start(): void {
    if (this.running || !this.renderer) return;
    this.running = true;
    const startTs = performance.now();
    const loop = (now: number) => {
      if (!this.running) return;
      this.material!.uniforms['u_time'].value = (now - startTs) / 1000;
      this.renderer!.render(this.scene!, this.camera!);
      this.rafId = requestAnimationFrame(loop);
    };
    this.rafId = requestAnimationFrame(loop);
  }

  private stop(): void {
    this.running = false;
    cancelAnimationFrame(this.rafId);
  }

  ngOnDestroy(): void {
    this.stop();
    if (this.onResize) window.removeEventListener('resize', this.onResize);
    if (this.onVisibility) document.removeEventListener('visibilitychange', this.onVisibility);
    this.observer?.disconnect();
    this.material?.dispose();
    this.renderer?.dispose();
  }
}
