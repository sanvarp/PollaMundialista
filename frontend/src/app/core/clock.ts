import { Injectable, signal } from '@angular/core';

/**
 * A single shared "now" signal that ticks every 30s, so many components can show
 * live countdowns without each owning a timer.
 */
@Injectable({ providedIn: 'root' })
export class Clock {
  private readonly _now = signal(Date.now());
  readonly now = this._now.asReadonly();

  constructor() {
    if (typeof setInterval !== 'undefined') {
      setInterval(() => this._now.set(Date.now()), 30_000);
    }
  }
}
