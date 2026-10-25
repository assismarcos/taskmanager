import { HttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { appConfig } from './app.config';
import { TokenStorage } from './core/auth/token-storage';

describe('app config', () => {
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [...appConfig.providers, provideHttpClientTesting()],
    });
  });

  afterEach(() => sessionStorage.clear());

  it('adds the bearer token to API calls', () => {
    // Arrange
    TestBed.inject(TokenStorage).write({
      accessToken: 'token-123',
      expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
      user: { id: '1', name: 'Ana', email: 'ana@example.com' },
    });
    const http = TestBed.inject(HttpClient);
    const httpTesting = TestBed.inject(HttpTestingController);

    // Act
    http.get('/api/tasks').subscribe();

    // Assert
    const request = httpTesting.expectOne('/api/tasks');
    expect(request.request.headers.get('Authorization')).toBe('Bearer token-123');
    request.flush({});
    httpTesting.verify();
  });
});
