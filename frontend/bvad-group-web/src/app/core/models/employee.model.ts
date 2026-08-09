export type Gender = 'Male' | 'Female' | 'Other';

export type ContractType =
  | 'CDI'
  | 'CDD'
  | 'Internship'
  | 'Freelance'
  | 'Apprenticeship';

export type EmployeeStatus =
  | 'Active'
  | 'OnLeave'
  | 'Suspended'
  | 'Terminated'
  | 'Probation';

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  middleName?: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  position: string;
  department?: string;
  status: EmployeeStatus;
  contractType: ContractType;
  hireDate: string;
  endDate?: string;
  salary?: number;
  birthDate?: string;
  age?: number;
  gender: Gender;
  city?: string;
  country?: string;
  photoUrl?: string;
  companyId: string;
  companyName: string;
  companyColor: string;
  companyLogo?: string;
  createdAt: string;
}

// Enum en valeurs numériques (pour envoyer au backend qui attend un int)
export const GenderValue = {
  Male: 0,
  Female: 1,
  Other: 2
} as const;

export const ContractTypeValue = {
  CDI: 0,
  CDD: 1,
  Internship: 2,
  Freelance: 3,
  Apprenticeship: 4
} as const;

export const EmployeeStatusValue = {
  Active: 0,
  OnLeave: 1,
  Suspended: 2,
  Terminated: 3,
  Probation: 4
} as const;

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  middleName?: string;
  email: string;
  phoneNumber?: string;
  position: string;
  department?: string;
  gender: number;
  birthDate?: string;
  hireDate: string;
  endDate?: string;
  contractType: number;
  salary?: number;
  status: number;
  city?: string;
  country?: string;
  companyId: string;
  photoUrl?: string;
  notes?: string;
}