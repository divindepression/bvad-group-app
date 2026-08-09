export type DocumentType =
  | 'Other' | 'CV' | 'NationalIdFront' | 'NationalIdBack' | 'Passport'
  | 'Diploma' | 'Certificate' | 'DrivingLicense' | 'MedicalCertificate'
  | 'WorkPermit' | 'Reference' | 'Contract' | 'Payslip'
  | 'BirthCertificate' | 'MarriageCertificate';

export interface EmployeeDocument {
  id: string;
  employeeId: string;
  type: DocumentType;
  title: string;
  description?: string;
  fileName: string;
  fileUrl: string;
  contentType?: string;
  fileSize: number;
  issueDate?: string;
  expiryDate?: string;
  isVerified: boolean;
  isExpired: boolean;
  isExpiringSoon: boolean;
  createdAt: string;
}

export const DocumentTypeValue = {
  Other: 0, CV: 1, NationalIdFront: 2, NationalIdBack: 3,
  Passport: 4, Diploma: 5, Certificate: 6, DrivingLicense: 7,
  MedicalCertificate: 8, WorkPermit: 9, Reference: 10,
  Contract: 11, Payslip: 12, BirthCertificate: 13, MarriageCertificate: 14
} as const;

export const DocumentTypeLabels: Record<DocumentType, string> = {
  Other: 'Autre',
  CV: 'CV / Curriculum Vitae',
  NationalIdFront: 'CNI (recto)',
  NationalIdBack: 'CNI (verso)',
  Passport: 'Passeport',
  Diploma: 'Diplôme',
  Certificate: 'Certificat',
  DrivingLicense: 'Permis de conduire',
  MedicalCertificate: 'Certificat médical',
  WorkPermit: 'Permis de travail',
  Reference: 'Lettre de recommandation',
  Contract: 'Contrat signé',
  Payslip: 'Fiche de paie',
  BirthCertificate: 'Acte de naissance',
  MarriageCertificate: 'Acte de mariage'
};

export const DocumentTypeIcons: Record<DocumentType, string> = {
  Other: '📎',
  CV: '📄',
  NationalIdFront: '🆔',
  NationalIdBack: '🆔',
  Passport: '📘',
  Diploma: '🎓',
  Certificate: '📜',
  DrivingLicense: '🚗',
  MedicalCertificate: '🏥',
  WorkPermit: '💼',
  Reference: '✉️',
  Contract: '📝',
  Payslip: '💰',
  BirthCertificate: '👶',
  MarriageCertificate: '💍'
};

export interface CreateDocumentMetadata {
  type: number;
  title: string;
  description?: string;
  issueDate?: string;
  expiryDate?: string;
}