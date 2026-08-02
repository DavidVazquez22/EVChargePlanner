import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { PriceRecord } from '../types/PriceRecord';

interface PriceChartProps {
  prices: PriceRecord[];
}

const PriceChart = ({ prices }: PriceChartProps) => {
  const chartData = prices.map((p) => ({
    time: new Date(p.timeStart).getHours() + ':00',
    price: p.pricePerKWh,
  }));

  return (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={chartData}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="time" />
        <YAxis label={{ value: 'NOK/kWh', angle: -90, position: 'insideLeft' }} />
        <Tooltip />
        <Line type="monotone" dataKey="price" stroke="#2563eb" strokeWidth={2} dot={false} />
      </LineChart>
    </ResponsiveContainer>
  );
};

export default PriceChart;