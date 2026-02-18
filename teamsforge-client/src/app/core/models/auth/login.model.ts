import { UserDto } from './register.model';

export interface LoginDto {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: UserDto;
}

export interface RefreshTokenDto {
  refreshToken: string;
}

export interface MessageResponseDto {
  message: string;
}

export interface ForgotPasswordDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  token: string;
  newPassword: string;
}

export interface UpdateProfileDto {
  firstName: string;
  lastName: string;
  displayName: string;
  avatarUrl?: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}

export interface EmailVerificationDto {
  token: string;
}
