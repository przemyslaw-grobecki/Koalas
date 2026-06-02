import { useState, useEffect } from 'react';
import {
  Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Typography, Box, LinearProgress,
} from '@mui/material';
import { getAllBamboo } from '../api';
import type { Bamboo } from '../types';

export default function BambooPanel() {
  const [bamboo, setBamboo] = useState<Bamboo[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getAllBamboo()
      .then(setBamboo)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <LinearProgress />;

  return (
    <Box>
      <Typography variant="h6" gutterBottom>Bamboo Plantation</Typography>
      <Typography variant="body2" color="text.secondary" gutterBottom>
        Total stalks: {bamboo.length}
      </Typography>
      <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>ID</TableCell>
              <TableCell>Species</TableCell>
              <TableCell>Height (cm)</TableCell>
              <TableCell>Diameter (cm)</TableCell>
              <TableCell>Weight (kg)</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Planted</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {bamboo.map((b) => (
              <TableRow key={b.id}>
                <TableCell>{b.id}</TableCell>
                <TableCell>{b.species}</TableCell>
                <TableCell>{b.heightCm}</TableCell>
                <TableCell>{b.diameterCm}</TableCell>
                <TableCell>{b.weightKg.toFixed(2)}</TableCell>
                <TableCell>{b.location}</TableCell>
                <TableCell>{new Date(b.plantedDate).toLocaleDateString()}</TableCell>
              </TableRow>
            ))}
            {bamboo.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} align="center">No bamboo found</TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}