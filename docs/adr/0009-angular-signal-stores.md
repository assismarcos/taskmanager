# 0009 — Use Angular signal-based stores and Angular Material

- Status: Accepted
- Date: 2026-10-25

## Context
The frontend has two features (auth and tasks) with modest shared state: the current user, the task page, the filter and the loading/error flags. It must be responsive and easy for a reviewer to follow. Angular 22 generates standalone, zoneless, signal-based apps by default.

## Decision
Each feature has one store: an injectable service holding state in `signal()` and `computed()` and exposing intention-revealing methods (`load()`, `create()`, ...). API services only do HTTP. Components read signals and call store methods. Angular Material provides the components and responsive building blocks (`BreakpointObserver`, `mat-table`, cards, datepicker, dialog, snackbar).

## Alternatives considered
- NgRx Store / Effects — powerful for large apps; here it adds actions, reducers and effects for a few pieces of state.
- NgRx SignalStore — closer in spirit, but one more library to learn for no extra capability at this size.
- RxJS `BehaviorSubject` services — the pre-signals idiom; more subscription management and less aligned with zoneless change detection.
- Tailwind or a custom component set — more styling freedom, more work for accessibility and responsiveness.

## Consequences
- Positive: idiomatic modern Angular; little code; easy to test stores with `HttpTestingController`; accessible components out of the box.
- Negative / accepted trade-offs: no time-travel debugging or devtools; Material's look is recognizable and less customizable.
