import FormField from "../../components/FormField";
import { useState } from "react";
import type { IncomeFormRow } from "./types";
import { currencyFormatter } from "../../utils/CurrencyFormatter";
import apiClient from "../../api/axiosClient";
import { getApiError } from "../../utils/apiErrors";
import { getRowErrors } from "../../utils/rowErrors";
export default function CreatePaycheckForm({
  onClose,
}: {
  onClose: () => void;
}) {
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [error, setError] = useState("");
  const [form, setForm] = useState({
    description: "",
    receivedDate: "",
  });
  const newRow = (): IncomeFormRow => ({
    id: crypto.randomUUID(),
    source: "",
    amount: "",
    receivedDate: "",
  });
  const removeIncome = (id: string) => {
    if (income.length === 1) return;
    setIncome(income.filter((item) => item.id !== id));
  };

  const addIncome = () => {
    setIncome([...income, newRow()]);
  };
  const [income, setIncome] = useState<IncomeFormRow[]>([newRow()]);

  const updateIncome = (id: string, field: string, value: string) => {
    setIncome(
      income.map((inc) => (inc.id === id ? { ...inc, [field]: value } : inc)),
    );
    setFieldErrors((prev) => ({ ...prev, [`${field}-${id}`]: "" }));
  };

  const HandleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    setForm({ ...form, [id]: value });
    setFieldErrors({ ...fieldErrors, [id]: "" });
  };

  async function CreatePaycheck() {
    const rowErrors: Record<string, string> = {};
    income.forEach((row) => {
      if (!row.source) rowErrors[`source-${row.id}`] = "Source is required.";
      if (!row.amount || Number(row.amount) <= 0)
        rowErrors[`amount-${row.id}`] = "Amount must be greater than 0.";
      if (!row.receivedDate)
        rowErrors[`receivedDate-${row.id}`] = "Received date is required.";
    });

    const topErrors = {
      ...(!form.description && { description: "Description is required." }),
      ...(!form.receivedDate && { receivedDate: "Received date is required." }),
    };

    if (Object.keys(topErrors).length || Object.keys(rowErrors).length) {
      setFieldErrors({ ...topErrors, ...rowErrors });
      return;
    }
    let paycheckData = {
      description: form.description,
      receivedDate: form.receivedDate || null,
      incomesToCreate: income.map((item) => ({
        source: item.source,
        amount: Number(item.amount),
        receivedDate: item.receivedDate || null,
      })),
    };
    try {
      await apiClient.post("/api/paychecks/CreatePaycheck", paycheckData);
      onClose();
    } catch (error) {
      const { message, fieldErrors: apiErrors } = getApiError(error);
      const rowErrors = getRowErrors(
        apiErrors,
        "incomesToCreate",
        income.map((row) => row.id),
        ["source", "amount", "receivedDate"],
      );
      setError(Object.keys(apiErrors).length ? "" : message);
      setFieldErrors({ ...apiErrors, ...rowErrors });
    }
  }

  return (
    <div className="w-full h-full">
      <form
        onSubmit={(e) => {
          e.preventDefault();
          CreatePaycheck();
        }}
      >
        <div className="grid grid-cols-2 gap-7">
          <FormField
            id="description"
            type="text"
            placeholder="Enter description"
            label="Description"
            error={fieldErrors.description}
            value={form.description}
            onChange={HandleChange}
          />
          <FormField
            id="receivedDate"
            type="date"
            placeholder="Enter received date"
            label="Received date"
            error={fieldErrors.receivedDate}
            value={form.receivedDate}
            onChange={HandleChange}
          />
        </div>
        <p className="text-xs text-gray-400 mb-5">
          Required · max 100 characters
        </p>

        <h3 className="font-semibold text-gray-700 mb-3">Income</h3>
        <div className="h-40 overflow-auto">
          {income.map((item) => (
            <div
              key={item.id}
              className="grid grid-cols-8 text-gray-400 text-xs gap-3 overflow-auto"
            >
              <div className="col-span-3">
                <FormField
                  id={`source-${item.id}`}
                  type="text"
                  placeholder="Enter source"
                  label="Source"
                  labelClassName="text-xs"
                  value={item.source}
                  error={fieldErrors[`source-${item.id}`]}
                  onChange={(e) =>
                    updateIncome(item.id, "source", e.target.value)
                  }
                />
              </div>
              <div className="col-span-2">
                <FormField
                  id={`amount-${item.id}`}
                  type="text"
                  placeholder="Enter amount"
                  label="Amount"
                  labelClassName="text-xs"
                  value={item.amount}
                  error={fieldErrors[`amount-${item.id}`]}
                  onChange={(e) =>
                    updateIncome(item.id, "amount", e.target.value)
                  }
                />
              </div>
              <div className="col-span-2">
                <FormField
                  id={`receivedDate-${item.id}`}
                  type="date"
                  placeholder="Enter received date"
                  label="Received date"
                  labelClassName="text-xs"
                  value={item.receivedDate}
                  error={fieldErrors[`receivedDate-${item.id}`]}
                  onChange={(e) =>
                    updateIncome(item.id, "receivedDate", e.target.value)
                  }
                />
              </div>
              <div className="col-span-1 mt-5">
                <button
                  type="button"
                  onClick={() => removeIncome(item.id)}
                  aria-label="Remove income"
                  className="text-2xl leading-none text-gray-500 hover:text-gray-700"
                >
                  &times;
                </button>
              </div>
            </div>
          ))}
        </div>

        <div>
          <button
            className=" text-blue-700"
            type="button"
            onClick={() => addIncome()}
          >
            + Add another income
          </button>
        </div>
        <div className="flex justify-between items-center border-t border-gray-200 mt-5 pt-5">
          <span className="text-gray-500">Total income</span>
          <span className="text-xl font-bold text-gray-900">
            {currencyFormatter(
              income.reduce((sum, item) => sum + Number(item.amount), 0),
            )}
          </span>
        </div>

        <div className="flex justify-end gap-3 mt-5">
          <button
            type="button"
            className="border border-gray-300 rounded-md px-5 py-2 text-gray-700"
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="bg-blue-600 text-white rounded-md px-5 py-2 font-medium"
          >
            Create Paycheck
          </button>
        </div>
        {error && (
          <p className="text-red-600 text-sm text-center mb-2">{error}</p>
        )}
      </form>
    </div>
  );
}
