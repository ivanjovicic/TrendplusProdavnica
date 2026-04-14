export function formatCurrency(amount: number, currency: string = 'RSD'): string {
  if (amount === undefined || amount === null) return '0,00 ' + currency;

  return new Intl.NumberFormat('sr-RS', {
    style: 'decimal',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount) + ' ' + currency;
}

export function formatNumber(num: number): string {
  return new Intl.NumberFormat('sr-RS').format(num);
}
