export type Paycheck = {
  id: number;
  description: string;
  totalIncome: number;
  receivedDate: string;
  isClosed: boolean;
};

export type Income={
  id: number;
  source: string;
  amount: string;
  receivedDate: string;
}
export type IncomeFormRow = { 
  id: string;
   source: string;
    amount: string; 
    receivedDate: string 
  };