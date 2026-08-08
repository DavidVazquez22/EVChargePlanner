import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { PriceRecord } from '../types/PriceRecord';

interface PriceChartProps {
  prices: PriceRecord[];
}

const PriceChart = ({ prices }: PriceChartProps) => {
  const chartData = prices.map((p) => {
    const date = new Date(p.timeStart);
    const isToday = date.toDateString() === new Date().toDateString();
    const dayLabel = isToday ? '' : date.toLocaleDateString(undefined, { day: '2-digit', month: '2-digit' }) + ' ';

    return {
      time: `${dayLabel}${date.getHours()}:00`,
      price: p.pricePerKWh,
    };
  });

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