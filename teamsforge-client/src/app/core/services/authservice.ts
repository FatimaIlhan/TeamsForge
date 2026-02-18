import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  RegisterDto,
  LoginDto,
  LoginResponse,
  MessageResponseDto,
  UserDto,
  ForgotPasswordDto,
  ResetPasswordDto,
  RefreshTokenDto,
  UpdateProfileDto,
  ChangePasswordDto,
  EmailVerificationDto,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class Authservice {
  private apiUrl = environment.apiUrl;
  private readonly tokenKey = 'token';
  private readonly refreshTokenKey = 'refreshToken';
  private readonly userKey = 'user';

  constructor(private http: HttpClient) {}

  register(model: RegisterDto): Observable<UserDto> {
    return this.http.post<UserDto>(
      `${this.apiUrl}/auth/register`,
      model
    );
  }

  login(model: LoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/auth/login`,
      model
    ).pipe(
      tap((response) => this.setSession(response))
    );
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available.');
    }

    const payload: RefreshTokenDto = { refreshToken };

    return this.http
      .post<LoginResponse>(`${this.apiUrl}/auth/refresh-token`, payload)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): Observable<MessageResponseDto> {
    return this.http
      .post<MessageResponseDto>(`${this.apiUrl}/auth/logout`, {})
      .pipe(tap(() => this.clearSession()));
  }

  clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
  }

  isLoggedIn(): boolean {
    const token = localStorage.getItem(this.tokenKey);
    return !!token;
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  getUser(): UserDto | null {
    const rawUser = localStorage.getItem(this.userKey);

    if (!rawUser) {
      return null;
    }

    return JSON.parse(rawUser) as UserDto;
  }

  setSession(response: LoginResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.refreshTokenKey, response.refreshToken);
    localStorage.setItem(this.userKey, JSON.stringify(response.user));
  }

  forgotPassword(email: string): Observable<MessageResponseDto> {
    const payload: ForgotPasswordDto = { email };

    return this.http.post<MessageResponseDto>(
      `${this.apiUrl}/auth/forgot-password`,
      payload
    );
  }

  resetPassword(model: ResetPasswordDto): Observable<MessageResponseDto> {
    return this.http.post<MessageResponseDto>(
      `${this.apiUrl}/auth/reset-password`,
      model
    );
  }

  getProfile(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/auth/profile`);
  }

  updateProfile(model: UpdateProfileDto): Observable<UserDto> {
    return this.http
      .put<UserDto>(`${this.apiUrl}/auth/profile`, model)
      .pipe(tap((user) => localStorage.setItem(this.userKey, JSON.stringify(user))));
  }

  changePassword(model: ChangePasswordDto): Observable<MessageResponseDto> {
    return this.http.post<MessageResponseDto>(
      `${this.apiUrl}/auth/change-password`,
      model
    );
  }

  verifyEmail(token: string): Observable<MessageResponseDto> {
    const payload: EmailVerificationDto = { token };
    return this.http.post<MessageResponseDto>(`${this.apiUrl}/auth/verify-email`, payload);
  }
}
