import { AUTH_ROUTES } from './auth.routes';

describe('AUTH_ROUTES', () => {
  it('gives the login and register pages a title', () => {
    // Act
    const titles = Object.fromEntries(AUTH_ROUTES.map((route) => [route.path, route.title]));

    // Assert
    expect(titles).toEqual({ login: 'Log in', register: 'Register' });
  });
});
