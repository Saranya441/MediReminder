import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  CalendarDays,
  Check,
  Clock3,
  Edit3,
  FileText,
  Pill,
  Plus,
  Trash2,
} from "lucide-react";
import Sidebar from "../../components/Sidebar/Sidebar";
import EditMedicineModal from "../../components/EditMedicineModal/EditMedicineModal";
import "./PrescriptionReview.css";
import { errorMessage } from "../../api/client";
import { medicinesApi, prescriptionsApi } from "../../api/services";

function PrescriptionReview() {
  const navigate = useNavigate();
  const location = useLocation();
  const prescriptionId = location.state?.prescriptionId;
  const [prescription, setPrescription] = useState(null);
  const [pageError, setPageError] = useState("");
  const [saving, setSaving] = useState(false);
  const [medicines, setMedicines] = useState(location.state?.reviewMedicines || []);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [selectedMedicine, setSelectedMedicine] = useState(null);

  useEffect(() => {
    const additions = location.state?.newMedicines;
    if (!additions?.length) return;
    const merged = [...(location.state.reviewMedicines || []), ...additions];
    setMedicines(merged);
    navigate(location.pathname, { replace: true, state: {
      prescriptionId: location.state.prescriptionId,
      reviewMedicines: merged,
    } });
  }, [location.state, location.pathname, navigate]);

  useEffect(() => {
    if (!prescriptionId) return;
    prescriptionsApi.get(prescriptionId)
      .then(setPrescription)
      .catch((error) => setPageError(errorMessage(error, "Could not load the uploaded prescription.")));
  }, [prescriptionId]);

  const handleEditMedicine = (medicine) => {
    setSelectedMedicine(medicine);
    setIsEditModalOpen(true);
  };

  const handleCloseModal = () => {
    setIsEditModalOpen(false);
    setSelectedMedicine(null);
  };

  const handleSaveMedicine = (updatedMedicine) => {
    setMedicines((current) =>
      current.map((medicine) =>
        medicine.id === updatedMedicine.id ? updatedMedicine : medicine,
      ),
    );

    handleCloseModal();
  };

  const handleDeleteMedicine = (id) => {
    setMedicines((current) => current.filter((medicine) => medicine.id !== id));
  };

  const handleConfirmPrescription = async () => {
    setSaving(true); setPageError("");
    try {
      await medicinesApi.create(medicines.map((medicine) => ({
        name: medicine.name, dosage: medicine.dosage, form: medicine.form || "Tablet",
        frequency: medicine.frequency || "As directed", foodInstruction: medicine.foodInstruction,
        startDate: medicine.startDate, endDate: medicine.endDate || null,
        times: medicine.times.filter(Boolean), reminderEnabled: true,
        instructions: medicine.instructions || null, status: "Active",
      })));
      navigate("/medicines");
    } catch (error) { setPageError(errorMessage(error, "Could not save the reviewed medicines.")); }
    finally { setSaving(false); }
  };

  return (
    <div className="prescription-review-layout">
      <Sidebar activePage="Prescriptions" />

      <main className="prescription-review-main">
        {/* PAGE HEADER */}

        <div className="prescription-review-header">
          <div>
            <p className="prescription-review-breadcrumb">
              Prescriptions / Review
            </p>

            <h1>Review Prescription</h1>

            <p>OCR is not enabled yet. Add the medicine details manually from your prescription before saving them.</p>
          </div>

          <div className="prescription-review-status">
            <FileText size={15} />
            {prescription ? "Uploaded · Manual review" : "Manual entry"}
          </div>
        </div>

        {/* SUMMARY */}

        <section className="prescription-summary-card">
          <div className="prescription-summary-icon">
            <FileText size={22} />
          </div>

          <div>
            <h2>Prescription Details</h2>

            <p>
              {prescription ? <>Uploaded file: <strong>{prescription.originalFileName}</strong>. No medicine details were automatically extracted.</> : <>Enter medicine details from your prescription. No OCR data has been added.</>}
            </p>
          </div>
        </section>
        {pageError && <p role="alert">{pageError}</p>}

        {/* MEDICINES */}

        <section className="prescription-medicine-section">
          <div className="prescription-section-heading">
            <div>
              <h2>Medicines</h2>

              <p>Enter and review each medicine yourself using the prescription as the source.</p>
            </div>

            <button
              type="button"
              className="prescription-add-medicine"
              onClick={() =>
                navigate("/add-medicine", {
                  state: {
                    from: "prescription-review",
                    prescriptionId,
                    reviewMedicines: medicines,
                  },
                })
              }
            >
              <Plus size={15} />
              Add Medicine
            </button>
          </div>

          <div className="prescription-medicine-list">
            {medicines.length === 0 ? (
              <div className="prescription-empty">
                <Pill size={25} />

                <h3>No medicines found</h3>

                <p>Add a medicine before confirming your prescription.</p>
              </div>
            ) : (
              medicines.map((medicine) => (
                <article
                  className="prescription-medicine-card"
                  key={medicine.id}
                >
                  <div className="prescription-medicine-top">
                    <div className="prescription-medicine-info">
                      <div className="prescription-pill-icon">
                        <Pill size={19} />
                      </div>

                      <div>
                        <h3>{medicine.name}</h3>

                        <span>{medicine.dosage}</span>
                      </div>
                    </div>

                    <div className="prescription-card-actions">
                      <button
                        type="button"
                        className="prescription-edit-button"
                        onClick={() => handleEditMedicine(medicine)}
                      >
                        <Edit3 size={14} />
                        Edit
                      </button>

                      <button
                        type="button"
                        className="prescription-delete-button"
                        onClick={() => handleDeleteMedicine(medicine.id)}
                        aria-label={`Delete ${medicine.name}`}
                      >
                        <Trash2 size={15} />
                      </button>
                    </div>
                  </div>

                  <div className="prescription-medicine-details">
                    <div>
                      <span className="prescription-detail-icon">
                        <Clock3 size={14} />
                      </span>

                      <div>
                        <small>Frequency</small>

                        <strong>{medicine.frequency}</strong>
                      </div>
                    </div>

                    <div>
                      <span className="prescription-detail-icon">
                        <Clock3 size={14} />
                      </span>

                      <div>
                        <small>Reminder Time</small>

                        <strong>{medicine.times.join(", ")}</strong>
                      </div>
                    </div>

                    <div>
                      <span className="prescription-detail-icon">
                        <CalendarDays size={14} />
                      </span>

                      <div>
                        <small>Duration</small>

                        <strong>
                          {medicine.startDate} - {medicine.endDate}
                        </strong>
                      </div>
                    </div>
                  </div>

                  {medicine.instructions && (
                    <div className="prescription-instructions">
                      <span>Instructions:</span>

                      {medicine.instructions}
                    </div>
                  )}
                </article>
              ))
            )}
          </div>
        </section>

        {/* FOOTER */}

        <div className="prescription-review-footer">
          <button
            type="button"
            className="prescription-back-button"
            onClick={() => navigate("/upload-prescription")}
          >
            Back
          </button>

          <button
            type="button"
            className="prescription-confirm-button"
            disabled={medicines.length === 0 || saving}
            onClick={handleConfirmPrescription}
          >
            <Check size={16} />
            {saving ? "Saving..." : "Confirm & Save"}
          </button>
        </div>
      </main>

      {/* REUSABLE MODAL */}

      <EditMedicineModal
        isOpen={isEditModalOpen}
        medicine={selectedMedicine}
        onClose={handleCloseModal}
        onSave={handleSaveMedicine}
      />
    </div>
  );
}

export default PrescriptionReview;
