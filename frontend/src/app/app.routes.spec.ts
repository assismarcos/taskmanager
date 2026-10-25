import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from './app.routes';

describe('app routes', () => {
  it('redirect unknown paths to /tasks', async () => {
    // Arrange
    TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
    const harness = await RouterTestingHarness.create();

    // Act
    await harness.navigateByUrl('/does-not-exist');

    // Assert
    expect(TestBed.inject(Router).url).toBe('/tasks');
  });
});
