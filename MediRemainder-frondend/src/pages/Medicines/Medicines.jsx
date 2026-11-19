import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
  CalendarDays,
  Clock3,
  Edit3,
  MoreVertical,
  Pill,
  Plus,
  Search,
  Trash2,
  XCircle,
} from "lucide-react";

import Sidebar from "../../components/Sidebar/Sidebar";
import EditMedicineModal from "../../components/EditMedicineModal/EditMedicineModal";
import MissedMedicineModal from "../../components/MissedMedicineModal/MissedMedicineModal";

import "./Medicines.css";
import { errorMessage } from "../../api/client";
import { historyApi, medicinesApi } from "../../api/services";

function Medicines() {
  const navigate = useNavigate();
  const [medicines, setMedicines] = useState([]);
  const [missedMedicines, setMissedMedicines] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState("");

  const [searchTerm, setSearchTerm] = useState("");

  const [selectedFilter, setSelectedFilter] = useState("All");

  const [selectedMedicine, setSelectedMedicine] = useState(null);

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);

  const [openMenuId, setOpenMenuId] = useState(null);

  /*
    NEW:
    Controls Missed Medicines popup.
  */
  const [isMissedModalOpen, setIsMissedModalOpen] = useState(false);

  const refresh = async () => {
    setLoading(true);
    try {
      const [medicineResult, missedResult] = await Promise.all([medicinesApi.list(), historyApi.missed()]);
      setMedicines(medicineResult?.medicines ?? []);
      setMissedMedicines(missedResult ?? []);
      setPageError("");
    } catch (requestError) {
      setPageError(errorMessage(requestError, "Could not load your medicines."));
    } finally { setLoading(false); }
  };

  useEffect(() => { refresh(); }, []);

  /*
    Search + status filtering
  */
  const filteredMedicines = useMemo(() => {
    return medicines.filter((medicine) => {
      const matchesSearch =
        medicine.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        medicine.dosage.toLowerCase().includes(searchTerm.toLowerCase());

      const matchesFilter =
        selectedFilter === "All" || medicine.status === selectedFilter;

      return matchesSearch && matchesFilter;
    });
  }, [medicines, searchTerm, selectedFilter]);

  /*
    SUMMARY COUNTS
  */
  const activeCount = medicines.filter(
    (medicine) => medicine.status === "Active",
  ).length;

  const completedCount = medicines.filter(
    (medicine) => medicine.status === "Completed",
  ).length;

  const missedCount = missedMedicines.length;

  /*
    EDIT
  */
  const handleEditMedicine = (medicine) => {
    setSelectedMedicine(medicine);
    setIsEditModalOpen(true);
    setOpenMenuId(null);
  };

  const handleCloseModal = () => {
    setSelectedMedicine(null);
    setIsEditModalOpen(false);
  };

  const handleSaveMedicine = async (updatedMedicine) => {
    try {
      await medicinesApi.update(updatedMedicine.id, {
        name: updatedMedicine.name, dosage: updatedMedicine.dosage,
        form: updatedMedicine.form, frequency: updatedMedicine.frequency,
        foodInstruction: updatedMedicine.foodInstruction || "As directed by doctor",
        startDate: updatedMedicine.startDate, endDate: updatedMedicine.endDate || null,
        times: updatedMedicine.times, reminderEnabled: updatedMedicine.reminderEnabled ?? true,
        instructions: updatedMedicine.instructions ?? null, status: updatedMedicine.status,
      });
      handleCloseModal();
      await refresh();
    } catch (requestError) { setPageError(errorMessage(requestError, "Could not update this medicine.")); }
  };

  /*
    DELETE
  */
  const handleDeleteMedicine = (medicine) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete ${medicine.name}?`,
    );

    if (!confirmed) {
      return;
    }

    medicinesApi.remove(medicine.id).then(refresh).catch((error) => setPageError(errorMessage(error, "Could not delete this medicine.")));
    setOpenMenuId(null);
  };

  /*
    For now display first configured time.

    Later actual reminder logic will calculate
    the nearest upcoming reminder.
  */
  const getNextReminder = (medicine) => {
    if (!medicine.times || medicine.times.length === 0) {
      return "No reminder";
    }

    return medicine.times[0];
  };

  return (
    <div className="medicines-layout">
      <Sidebar activePage="Medicines" />

      <main className="medicines-main">
        {/* =====================================
            HEADER
        ====================================== */}

        <header className="medicines-header">
          <div>
            <p className="medicines-breadcrumb">Medicines</p>

            <h1>My Medicines</h1>

            <p className="medicines-description">
              Manage your medicines, schedules and reminders in one place.
            </p>
          </div>

          <button
            type="button"
            className="medicines-add-button"
            onClick={() => navigate("/add-medicine")}
          >
            <Plus size={16} />
            Add Medicine
          </button>
        </header>

        {/* =====================================
            SUMMARY CARDS
        ====================================== */}

        <section className="medicines-summary">
          {/* TOTAL */}

          <div className="medicine-summary-card">
            <div className="medicine-summary-icon blue">
              <Pill size={19} />
            </div>

            <div>
              <span>Total Medicines</span>

              <strong>{medicines.length}</strong>
            </div>
          </div>

          {/* ACTIVE */}

          <div className="medicine-summary-card">
            <div className="medicine-summary-icon green">
              <Clock3 size={19} />
            </div>

            <div>
              <span>Active</span>

              <strong>{activeCount}</strong>
            </div>
          </div>

          {/* COMPLETED */}

          <div className="medicine-summary-card">
            <div className="medicine-summary-icon purple">
              <CalendarDays size={19} />
            </div>

            <div>
              <span>Completed</span>

              <strong>{completedCount}</strong>
            </div>
          </div>

          {/* NEW MISSED CARD */}

          <div
            className="medicine-summary-card medicine-summary-missed"
            role="button"
            tabIndex={0}
            onClick={() => setIsMissedModalOpen(true)}
            onKeyDown={(event) => {
              if (event.key === "Enter" || event.key === " ") {
                setIsMissedModalOpen(true);
              }
            }}
          >
            <div className="medicine-summary-icon red">
              <XCircle size={19} />
            </div>

            <div>
              <span>Missed</span>

              <strong>{missedCount}</strong>
            </div>
          </div>
        </section>

        {pageError && <p role="alert">{pageError}</p>}
        {loading && <p role="status">Loading your medicines...</p>}

        {/* =====================================
            SEARCH + FILTER
        ====================================== */}

        <section className="medicines-toolbar">
          <div className="medicines-search">
            <Search size={16} />

            <input
              type="text"
              placeholder="Search medicines..."
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
            />
          </div>

          <div className="medicines-filters">
            {["All", "Active", "Completed"].map((filter) => (
              <button
                type="button"
                key={filter}
                className={selectedFilter === filter ? "active" : ""}
                onClick={() => setSelectedFilter(filter)}
              >
                {filter}
              </button>
            ))}
          </div>
        </section>

        {/* =====================================
            TITLE
        ====================================== */}

        <div className="medicines-list-heading">
          <div>
            <h2>Your Medicines</h2>

            <p>
              {filteredMedicines.length} medicine
              {filteredMedicines.length !== 1 ? "s" : ""} found
            </p>
          </div>
        </div>

        {/* =====================================
            MEDICINE GRID
        ====================================== */}

        {!loading && filteredMedicines.length > 0 ? (
          <section className="medicines-grid">
            {filteredMedicines.map((medicine) => (
              <article className="medicine-list-card" key={medicine.id}>
                {/* CARD HEADER */}

                <div className="medicine-card-header">
                  <div className="medicine-card-name">
                    <div className="medicine-card-pill">
                      <Pill size={18} />
                    </div>

                    <div>
                      <h3>{medicine.name}</h3>

                      <p>{medicine.dosage}</p>
                    </div>
                  </div>

                  {/* THREE DOT MENU */}

                  <div className="medicine-menu-wrapper">
                    <button
                      type="button"
                      className="medicine-menu-button"
                      onClick={() =>
                        setOpenMenuId(
                          openMenuId === medicine.id ? null : medicine.id,
                        )
                      }
                      aria-label={`Options for ${medicine.name}`}
                    >
                      <MoreVertical size={17} />
                    </button>

                    {openMenuId === medicine.id && (
                      <div className="medicine-menu">
                        <button
                          type="button"
                          onClick={() => handleEditMedicine(medicine)}
                        >
                          <Edit3 size={14} />
                          Edit
                        </button>

                        <button
                          type="button"
                          className="delete"
                          onClick={() => handleDeleteMedicine(medicine)}
                        >
                          <Trash2 size={14} />
                          Delete
                        </button>
                      </div>
                    )}
                  </div>
                </div>

                {/* STATUS */}

                <div className="medicine-card-status-row">
                  <span
                    className={`medicine-status ${
                      medicine.status === "Active" ? "active" : "completed"
                    }`}
                  >
                    <span />

                    {medicine.status}
                  </span>
                </div>

                {/* DETAILS */}

                <div className="medicine-card-details">
                  {/* FREQUENCY */}

                  <div className="medicine-card-detail">
                    <div className="medicine-detail-icon">
                      <Clock3 size={14} />
                    </div>

                    <div>
                      <span>Frequency</span>

                      <strong>{medicine.frequency}</strong>
                    </div>
                  </div>

                  {/* NEXT REMINDER */}

                  <div className="medicine-card-detail">
                    <div className="medicine-detail-icon">
                      <Clock3 size={14} />
                    </div>

                    <div>
                      <span>Next Reminder</span>

                      <strong>
                        {medicine.status === "Completed"
                          ? "Completed"
                          : getNextReminder(medicine)}
                      </strong>
                    </div>
                  </div>

                  {/* DURATION */}

                  <div className="medicine-card-detail">
                    <div className="medicine-detail-icon">
                      <CalendarDays size={14} />
                    </div>

                    <div>
                      <span>Duration</span>

                      <strong>
                        {medicine.startDate}
                        {" - "}
                        {medicine.endDate}
                      </strong>
                    </div>
                  </div>
                </div>

                {/* REMINDER TIMES */}

                <div className="medicine-reminders">
                  <span>Reminder Times</span>

                  <div className="medicine-time-chips">
                    {medicine.times?.map((time, index) => (
                      <span key={`${medicine.id}-${index}`}>
                        <Clock3 size={11} />

                        {time}
                      </span>
                    ))}
                  </div>
                </div>

                {/* INSTRUCTIONS */}

                {medicine.instructions && (
                  <div className="medicine-card-instructions">
                    <strong>Instructions</strong>

                    <p>{medicine.instructions}</p>
                  </div>
                )}

                {/* CARD FOOTER */}

                <div className="medicine-card-footer">
                  <button
                    type="button"
                    onClick={() => handleEditMedicine(medicine)}
                  >
                    <Edit3 size={14} />
                    Edit Medicine
                  </button>
                </div>
              </article>
            ))}
          </section>
        ) : !loading ? (
          <section className="medicines-empty">
            <div className="medicines-empty-icon">
              <Pill size={26} />
            </div>

            <h2>No medicines found</h2>

            <p>
              {searchTerm || selectedFilter !== "All"
                ? "Try changing your search or filter."
                : "Add your first medicine to start tracking your medication."}
            </p>

            {!searchTerm && selectedFilter === "All" && (
              <button type="button" onClick={() => navigate("/add-medicine")}>
                <Plus size={15} />
                Add Medicine
              </button>
            )}
          </section>
        ) : null}
      </main>

      {/* =====================================
          EDIT MEDICINE POPUP
      ====================================== */}

      <EditMedicineModal
        isOpen={isEditModalOpen}
        medicine={selectedMedicine}
        onClose={handleCloseModal}
        onSave={handleSaveMedicine}
      />

      {/* =====================================
          MISSED MEDICINE POPUP
      ====================================== */}

      <MissedMedicineModal
        isOpen={isMissedModalOpen}
        missedMedicines={missedMedicines}
        onClose={() => setIsMissedModalOpen(false)}
      />
    </div>
  );
}

export default Medicines;
