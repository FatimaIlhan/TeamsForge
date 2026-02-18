export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  displayName: string;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  avatarUrl?: string;
  createdAt: string;
  updatedAt: string;
  lastLoginAt?: string;
  timeZone: string;
  language: string;
  emailConfirmed: boolean;
}
