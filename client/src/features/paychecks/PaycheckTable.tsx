import type { Paycheck } from "./types";
import { dateFormatter } from "../../utils/DateFormatter";
import { currencyFormatter } from "../../utils/CurrencyFormatter";
import { Badge } from "../../components/Badge";
import { Table } from "../../components/Table";
export function PaycheckTable({ paychecks }: { paychecks: Paycheck[] }) {
  return (
    <Table
      header={
        <tr>
          <th className="text-left font-medium p-2">Description</th>
          <th className="text-left font-medium p-2">Received Date</th>
          <th className="text-left font-medium p-2">Amount</th>
          <th className="text-left font-medium p-2">Status</th>
        </tr>
      }
      body={paychecks.map((paycheck) => (
        <tr
          className="border border-gray-300 hover:bg-gray-50"
          key={paycheck.id}
        >
          <td className=" p-2">{paycheck.description}</td>
          <td className=" p-2">{dateFormatter(paycheck.receivedDate)}</td>
          <td className=" p-2">{currencyFormatter(paycheck.totalIncome)}</td>
          <td className=" p-2">
            <Badge variant={paycheck.isClosed ? "neutral" : "success"}>
              {paycheck.isClosed ? "Closed" : "Open"}
            </Badge>
          </td>
        </tr>
      ))}
    />
  );
}
