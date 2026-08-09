export interface LoginRequest {
  username: string;
  password: string;
}

export interface UserDto {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string;
  photoUrl?: string;
}

export interface CompanyAccessDto {
  id: string;
  code: string;
  name: string;
  color: string;
  logo?: string;
  isHolding: boolean;
  isDefault: boolean;
  role?: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserDto;
  companies: CompanyAccessDto[];
}