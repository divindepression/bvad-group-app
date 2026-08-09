export interface Company {
  id: string;
  code: string;
  name: string;
  legalName?: string;
  description?: string;
  slogan?: string;
  color: string;
  logo?: string;              // emoji fallback
  logoUrl?: string;
  stampUrl?: string;
  directorSignatureUrl?: string;
  registrationNumber?: string;
  taxNumber?: string;
  address?: string;
  city?: string;
  country?: string;
  phone?: string;
  email?: string;
  website?: string;
  directorName?: string;
  directorTitle?: string;
  isHolding: boolean;
  displayOrder: number;
  isActive: boolean;
}

export interface UpdateCompanyRequest {
  name: string;
  legalName?: string;
  description?: string;
  slogan?: string;
  color: string;
  registrationNumber?: string;
  taxNumber?: string;
  address?: string;
  city?: string;
  country?: string;
  phone?: string;
  email?: string;
  website?: string;
  directorName?: string;
  directorTitle?: string;
}