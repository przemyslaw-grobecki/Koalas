export interface Koala {
  id: number;
  name: string;
  ageYears: number;
  ageDays: number;
  gender: string;
  hungerLevel: number;
  status: string;
  isAlive: boolean;
  createdAt: string;
}

export interface Bamboo {
  id: number;
  species: string;
  heightCm: number;
  diameterCm: number;
  location: string;
  weightKg: number;
  plantedDate: string;
  createdAt: string;
}

export interface KoalaStats {
  totalAlive: number;
  totalDead: number;
  totalAll: number;
  byStatus: { status: string; count: number }[];
  averageAge: number;
  averageHunger: number;
}

export interface BambooStats {
  totalStalkCount: number;
  totalWeightKg: number;
  averageWeightPerStalk: number;
  averageHeightCm: number;
  averageDiameterCm: number;
}