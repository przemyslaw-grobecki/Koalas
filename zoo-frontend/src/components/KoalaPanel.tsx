import { useState, useEffect } from 'react';
import {
  Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Typography, Button, Chip, Alert, Snackbar, Box, LinearProgress,
} from '@mui/material';
import { getKoalas, feedKoala } from '../api';
import type { Koala } from '../types';

export default function KoalaPanel() {
  const [koalas, setKoalas] = useState<Koala[]>([]);
  const [loading, setLoading] = useState(true);
  const [feedingIds, setFeedingIds] = useState<Set<number>>(new Set());
  const [snack, setSnack] = useState<{ msg: string; severity: 'success' | 'error' } | null>(null);

  const load = async () => {
    try {
      setKoalas(await getKoalas());
    } catch {
      setSnack({ msg: 'Failed to load koalas', severity: 'error' });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleFeed = async (id: number) => {
    setFeedingIds((prev) => new Set(prev).add(id));
    try {
      const result = await feedKoala(id);
      setSnack({ msg: result.message, severity: 'success' });
      await load();
    } catch (err: any) {
      setSnack({ msg: err.message || 'Failed to feed koala', severity: 'error' });
    } finally {
      setFeedingIds((prev) => { const next = new Set(prev); next.delete(id); return next; });
    }
  };

  const statusColor = (s: string) => {
    switch (s) {
      case 'Healthy': return 'success';
      case 'Hungry': return 'warning';
      case 'Starving': return 'error';
      default: return 'default';
    }
  };

  if (loading) return <LinearProgress />;

  return (
    <Box>
      <Typography variant="h6" gutterBottom>Koalas</Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Total alive: {koalas.length}
      </Typography>
      <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Name</TableCell>
              <TableCell>Age</TableCell>
              <TableCell>Gender</TableCell>
              <TableCell>Hunger</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Action</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {koalas.map((k) => (
              <TableRow key={k.id}>
                <TableCell>{k.id}</TableCell>
                <TableCell>{k.name}</TableCell>
                <TableCell>{k.ageYears}y {k.ageDays}d</TableCell>
                <TableCell>{k.gender}</TableCell>
                <TableCell>{k.hungerLevel}</TableCell>
                <TableCell>
                  <Chip label={k.status} color={statusColor(k.status) as any} size="small" />
                </TableCell>
                <TableCell>
                  <Button
                    size="small"
                    variant="contained"
                    disabled={feedingIds.has(k.id) || k.hungerLevel === 0}
                    onClick={() => handleFeed(k.id)}
                  >
                    {feedingIds.has(k.id) ? 'Feeding...' : 'Feed'}
                  </Button>
                </TableCell>
              </TableRow>
            ))}
            {koalas.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} align="center">No koalas found</TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <Snackbar open={!!snack} autoHideDuration={3000} onClose={() => setSnack(null)}>
        <Alert severity={snack?.severity ?? 'success'} onClose={() => setSnack(null)} sx={{ width: '100%' }}>
          {snack?.msg}
        </Alert>
      </Snackbar>
    </Box>
  );
}