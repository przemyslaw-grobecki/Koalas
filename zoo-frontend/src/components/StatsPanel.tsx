import { useState, useEffect } from 'react';
import {
  Card, CardContent, Grid, Typography, Box, LinearProgress,
} from '@mui/material';
import { getKoalaStats, getBambooStats, getBambooTotalWeight } from '../api';
import type { KoalaStats, BambooStats } from '../types';

export default function StatsPanel() {
  const [koalaStats, setKoalaStats] = useState<KoalaStats | null>(null);
  const [bambooStats, setBambooStats] = useState<BambooStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      getKoalaStats(),
      getBambooStats(),
    ]).then(([ks, bs]) => {
      setKoalaStats(ks);
      setBambooStats(bs);
    }).finally(() => setLoading(false));
  }, []);

  if (loading) return <LinearProgress />;

  return (
    <Box>
      <Typography variant="h6" gutterBottom>Zoo Statistics</Typography>
      <Grid container spacing={2}>
        {/* Koala stats */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" gutterBottom>Koalas</Typography>
              {koalaStats && (
                <>
                  <Typography variant="body2">Alive: {koalaStats.totalAlive}</Typography>
                  <Typography variant="body2">Dead: {koalaStats.totalDead}</Typography>
                  <Typography variant="body2">Total: {koalaStats.totalAll}</Typography>
                  <Typography variant="body2">Avg Age: {koalaStats.averageAge.toFixed(1)} years</Typography>
                  <Typography variant="body2">Avg Hunger: {koalaStats.averageHunger.toFixed(1)}</Typography>
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="caption">By Status:</Typography>
                    {koalaStats.byStatus.map((s) => (
                      <Typography key={s.status} variant="caption" display="block">
                        {s.status}: {s.count}
                      </Typography>
                    ))}
                  </Box>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Bamboo stats */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="subtitle1" gutterBottom>Bamboo</Typography>
              {bambooStats && (
                <>
                  <Typography variant="body2">Stalks: {bambooStats.totalStalkCount}</Typography>
                  <Typography variant="body2">Total Weight: {bambooStats.totalWeightKg.toFixed(2)} kg</Typography>
                  <Typography variant="body2">Avg Weight: {bambooStats.averageWeightPerStalk.toFixed(2)} kg</Typography>
                  <Typography variant="body2">Avg Height: {bambooStats.averageHeightCm.toFixed(1)} cm</Typography>
                  <Typography variant="body2">Avg Diameter: {bambooStats.averageDiameterCm.toFixed(1)} cm</Typography>
                </>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}