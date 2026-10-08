import { useEffect, useState } from "react";
import apiClient from "../../api/axiosClient";
import EmptyTable from "../../components/EmptyTable";
import { PaycheckTable } from "./PaycheckTable";
import type { Paycheck } from "./types";
import { LoadingOverlay } from "../../components/LoadingOverlay";
import Modal from "../../components/Modal";
import CreatePaycheckForm from "./CreatePaycheckForm";
export default function PaychecksPage() {
  const [paychecks, setPaychecks] = useState<Paycheck[]>([]);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setModalOpen] = useState(false);
  useEffect(() => {
    setIsLoading(true);
    apiClient
      .get("/api/Paychecks/GetPaychecks")
      .then((res) => {
        setPaychecks(res?.data?.data?.items);
        setIsLoading(false);
      })
      .catch((error) => {
        setIsLoading(false);
        console.error(error);
        setError("Failed to load paychecks. Please try again later.");
      });
  }, []);

  return (
    <div className="p-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold mb-4">Paychecks</h1>
        {error && <p className="text-red-500">{error}</p>}
        <button
          className="bg-blue-600 text-white p-4 rounded-lg hover:bg-blue-700 border-none mb-4 cursor-pointer"
          onClick={() => setModalOpen(true)}
        >
          + New Paycheck
        </button>
      </div>
      {isModalOpen && (
        <Modal
          isOpen={isModalOpen}
          onClose={() => setModalOpen(false)}
          title="New Paycheck"
        >
          <CreatePaycheckForm onClose={() => setModalOpen(false)} />
        </Modal>
      )}
      {paychecks.length === 0 && !isLoading ? (
        <EmptyTable
          caption="No paychecks yet"
          description="Create your first paycheck to start planning where your money goes before you spend it."
          buttonText="+ Create your first paycheck"
          onClick={() => setModalOpen(true)}
        />
      ) : (
        <LoadingOverlay isLoading={isLoading}>
          {<PaycheckTable paychecks={paychecks} />}
        </LoadingOverlay>
      )}
    </div>
  );
}
