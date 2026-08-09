export type Gender = 'Male' | 'Female' | 'Other';

export type ContractType =
  | 'CDI' | 'CDD' | 'Internship' | 'Freelance' | 'Apprenticeship';

export type EmployeeStatus =
  | 'Active' | 'OnLeave' | 'Suspended' | 'Terminated' | 'Probation';

export type UserRoleType =
  | 'SuperAdmin' | 'Admin' | 'User'
  | 'Director' | 'Manager' | 'HR' | 'Accountant' | 'Employee';

export type CommitteePositionType =
  | 'None' | 'CEO' | 'DGA' | 'CFO' | 'CHRO'
  | 'CTO' | 'COO' | 'CMO' | 'CIO' | 'Legal' | 'Custom';

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
  companyRole: UserRoleType;
  isCommitteeMember: boolean;
  committeePosition: CommitteePositionType;
  committeePositionCustom?: string;
  managerId?: string;
  managerName?: string;
  userId?: string;
  createdAt: string;
}

// Enums numériques (envoi au backend)
export const GenderValue = { Male: 0, Female: 1, Other: 2 } as const;

export const ContractTypeValue = {
  CDI: 0, CDD: 1, Internship: 2, Freelance: 3, Apprenticeship: 4
} as const;

export const EmployeeStatusValue = {
  Active: 0, OnLeave: 1, Suspended: 2, Terminated: 3, Probation: 4
} as const;

export const UserRoleValue = {
  SuperAdmin: 0, Admin: 1, User: 2,
  Director: 10, Manager: 11, HR: 12, Accountant: 13, Employee: 14
} as const;

export const CommitteePositionValue = {
  None: 0, CEO: 1, DGA: 2, CFO: 3, CHRO: 4,
  CTO: 5, COO: 6, CMO: 7, CIO: 8, Legal: 9, Custom: 99
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
  companyRole: number;
  isCommitteeMember: boolean;
  committeePosition: number;
  committeePositionCustom?: string;
  managerId?: string;
}

// Helpers d'affichage
export const CommitteePositionLabels: Record<CommitteePositionType, string> = {
  None: '—',
  CEO: 'Président-Directeur Général',
  DGA: 'Directeur Général Adjoint',
  CFO: 'Directeur Financier',
  CHRO: 'Directeur RH',
  CTO: 'Directeur Technique',
  COO: 'Directeur des Opérations',
  CMO: 'Directeur Marketing/Commercial',
  CIO: 'Directeur des SI',
  Legal: 'Directeur Juridique',
  Custom: 'Poste personnalisé'
};

export const CommitteePositionIcons: Record<CommitteePositionType, string> = {
  None: '👤',
  CEO: '👑',
  DGA: '⭐',
  CFO: '💰',
  CHRO: '💼',
  CTO: '🎯',
  COO: '⚙️',
  CMO: '📈',
  CIO: '💻',
  Legal: '⚖️',
  Custom: '🎖'
};

export const UserRoleLabels: Record<UserRoleType, string> = {
  SuperAdmin: 'Super Administrateur',
  Admin: 'Administrateur',
  User: 'Utilisateur',
  Director: 'Directeur',
  Manager: 'Manager',
  HR: 'Ressources Humaines',
  Accountant: 'Comptable',
  Employee: 'Employé'
};