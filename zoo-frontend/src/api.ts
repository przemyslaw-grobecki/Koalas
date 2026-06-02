import type { Koala, Bamboo, KoalaStats, BambooStats } from './types';

const KOALA_API = '/api/koalas';
const BAMBOO_API = '/api/bamboo';

async function fetchJson<T>(url: string): Promise<T> {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
  return res.json();
}

// ---- Koala endpoints ----
export async function getKoalas(): Promise<Koala[]> {
  return fetchJson<Koala[]>(KOALA_API);
}

export async function getKoalaById(id: number): Promise<Koala | null> {
  try {
    return await fetchJson<Koala>(`${KOALA_API}/${id}`);
  } catch {
    return null;
  }
}

export async function getKoalaStats(): Promise<KoalaStats> {
  return fetchJson<KoalaStats>(`${KOALA_API}/stats`);
}

export async function feedKoala(id: number): Promise<{ message: string; koala: Koala }> {
  const res = await fetch(`${KOALA_API}/feed/${id}`, { method: 'POST' });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }
  return res.json();
}

// ---- Bamboo endpoints ----
export async function getAllBamboo(): Promise<Bamboo[]> {
  return fetchJson<Bamboo[]>(BAMBOO_API);
}

export async function getBambooStats(): Promise<BambooStats> {
  return fetchJson<BambooStats>(`${BAMBOO_API}/stats`);
}

export async function getBambooTotalWeight(): Promise<{ totalWeightKg: number; stalksCount: number }> {
  return fetchJson<{ totalWeightKg: number; stalksCount: number }>(`${BAMBOO_API}/total-weight`);
}

export async function harvestBamboo(weight: number): Promise<{ consumedKg: number; consumedBambooIds: number[] }> {
  const res = await fetch(`${BAMBOO_API}/harvest/${weight}`, { method: 'POST' });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }
  return res.json();
}