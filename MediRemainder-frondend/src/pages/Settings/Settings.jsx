import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
  Bell,
  ChevronRight,
  Clock3,
  LockKeyhole,
  LogOut,
  Moon,
  ShieldCheck,
  Smartphone,
  Trash2,
  Volume2,
} from "lucide-react";

import Sidebar from "../../components/Sidebar/Sidebar";
import "./Settings.css";
import { clearSession, errorMessage } from "../../api/client";
import { profileApi, settingsApi } from "../../api/services";
import { applyAppearance } from "../../theme";

function Settings() {
  const navigate = useNavigate();
  const [settings, setSettings] = useState({
    medicineReminders: true,
    missedDoseAlerts: true,
    prescriptionReminders: true,
    vibration: true,
    sound: true,
    reminderTime: "10",
    appearance: "system",
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    settingsApi.get().then((result) => {
      const appearance = ["light", "dark", "system"].includes(result.appearance)
        ? result.appearance
        : "system";
      applyAppearance(appearance, { persist: true });
      setSettings({
        medicineReminders: result.medicineReminders,
        missedDoseAlerts: result.missedDoseAlerts,
        prescriptionReminders: result.prescriptionReminders,
        vibration: result.vibrationEnabled,
        sound: result.soundEnabled,
        reminderTime: String(result.advanceReminderMinutes),
        appearance,
      });
    }).catch((requestError) => setError(errorMessage(requestError, "Could not load settings.")))
      .finally(() => setLoading(false));
  }, []);

  const handleToggle = (name) => {
    setSettings((previous) => ({
      ...previous,
      [name]: !previous[name],
    }));
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    setSettings((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const handleAppearanceChange = (appearance) => {
    applyAppearance(appearance, { persist: true });
    setSettings((previous) => ({ ...previous, appearance }));
  };

  const handleSave = async () => {
    setSaving(true); setError(""); setMessage("");
    try {
      await settingsApi.update({
        medicineReminders: settings.medicineReminders,
        missedDoseAlerts: settings.missedDoseAlerts,
        prescriptionReminders: settings.prescriptionReminders,
        soundEnabled: settings.sound,
        vibrationEnabled: settings.vibration,
        advanceReminderMinutes: Number(settings.reminderTime),
        appearance: settings.appearance,
      });
      setMessage("Settings saved successfully.");
    } catch (requestError) { setError(errorMessage(requestError, "Could not save settings.")); }
    finally { setSaving(false); }
  };

  const handleChangePassword = async () => {
    const currentPassword = window.prompt("Enter your current password:");
    if (!currentPassword) return;
    const newPassword = window.prompt("Enter your new password (at least 8 characters):");
    if (!newPassword) return;
    try {
      await profileApi.changePassword({ currentPassword, newPassword });
      setError(""); setMessage("Password changed. Sign in again after your current session expires.");
    } catch (requestError) { setError(errorMessage(requestError, "Could not change the password.")); }
  };

  const handleDeleteAccount = async () => {
    const confirmation = window.prompt("This permanently deletes your account and health data. Type DELETE to continue:");
    if (confirmation !== "DELETE") return;
    const currentPassword = window.prompt("Enter your current password to confirm:");
    if (!currentPassword) return;
    try {
      await profileApi.deleteAccount({ currentPassword, confirmation });
      clearSession();
      navigate("/", { replace: true });
    } catch (requestError) { setError(errorMessage(requestError, "Could not delete the account.")); }
  };

  return (
    <div className="settings-page-layout">
      <Sidebar activePage="Settings" />

      <main className="settings-page-main">
        {/* HEADER */}

        <section className="settings-page-header">
          <div>
            <p className="settings-breadcrumb">Settings</p>

            <h1>Settings</h1>

            <p className="settings-subtitle">
              Customize reminders, notifications, appearance, and account
              preferences.
            </p>
          </div>

          <button
            type="button"
            className="settings-save-button"
            onClick={handleSave}
            disabled={saving}
          >
            {saving ? "Saving..." : "Save Changes"}
          </button>
        </section>

        {(loading || error || message) && <p role={error ? "alert" : "status"}>{loading ? "Loading settings..." : error || message}</p>}

        {/* MAIN GRID */}

        <section className="settings-content-grid">
          <div className="settings-main-column">
            {/* NOTIFICATIONS */}

            <div className="settings-card">
              <div className="settings-card-heading">
                <div className="settings-heading-icon">
                  <Bell size={21} />
                </div>

                <div>
                  <h2>Notifications</h2>

                  <p>Manage medicine and health alerts</p>
                </div>
              </div>

              <div className="settings-list">
                <div className="settings-row">
                  <div className="settings-row-info">
                    <div className="settings-row-icon">
                      <Bell size={18} />
                    </div>

                    <div>
                      <h3>Medicine Reminders</h3>

                      <p>
                        Receive alerts when it is time to take your medicine.
                      </p>
                    </div>
                  </div>

                  <button
                    type="button"
                    className={`settings-toggle ${
                      settings.medicineReminders ? "active" : ""
                    }`}
                    onClick={() => handleToggle("medicineReminders")}
                    aria-label="Toggle medicine reminders"
                  >
                    <span />
                  </button>
                </div>

                <div className="settings-row">
                  <div className="settings-row-info">
                    <div className="settings-row-icon danger">
                      <Bell size={18} />
                    </div>

                    <div>
                      <h3>Missed Dose Alerts</h3>

                      <p>Get notified when a scheduled dose is missed.</p>
                    </div>
                  </div>

                  <button
                    type="button"
                    className={`settings-toggle ${
                      settings.missedDoseAlerts ? "active" : ""
                    }`}
                    onClick={() => handleToggle("missedDoseAlerts")}
                    aria-label="Toggle missed dose alerts"
                  >
                    <span />
                  </button>
                </div>

                <div className="settings-row">
                  <div className="settings-row-info">
                    <div className="settings-row-icon purple">
                      <Smartphone size={18} />
                    </div>

                    <div>
                      <h3>Prescription Reminders</h3>

                      <p>
                        Receive reminders related to prescriptions and renewals.
                      </p>
                    </div>
                  </div>

                  <button
                    type="button"
                    className={`settings-toggle ${
                      settings.prescriptionReminders ? "active" : ""
                    }`}
                    onClick={() => handleToggle("prescriptionReminders")}
                    aria-label="Toggle prescription reminders"
                  >
                    <span />
                  </button>
                </div>
              </div>
            </div>

            {/* REMINDER PREFERENCES */}

            <div className="settings-card">
              <div className="settings-card-heading">
                <div className="settings-heading-icon green">
                  <Clock3 size={21} />
                </div>

                <div>
                  <h2>Reminder Preferences</h2>

                  <p>Control how medicine reminders behave.</p>
                </div>
              </div>

              <div className="settings-list">
                <div className="settings-row">
                  <div className="settings-row-info">
                    <div className="settings-row-icon">
                      <Volume2 size={18} />
                    </div>

                    <div>
                      <h3>Reminder Sound</h3>

                      <p>Play a sound when a medicine reminder appears.</p>
                    </div>
                  </div>

                  <button
                    type="button"
                    className={`settings-toggle ${
                      settings.sound ? "active" : ""
                    }`}
                    onClick={() => handleToggle("sound")}
                    aria-label="Toggle reminder sound"
                  >
                    <span />
                  </button>
                </div>

                <div className="settings-row">
                  <div className="settings-row-info">
                    <div className="settings-row-icon">
                      <Smartphone size={18} />
                    </div>

                    <div>
                      <h3>Vibration</h3>

                      <p>Vibrate your device when a reminder is triggered.</p>
                    </div>
                  </div>

                  <button
                    type="button"
                    className={`settings-toggle ${
                      settings.vibration ? "active" : ""
                    }`}
                    onClick={() => handleToggle("vibration")}
                    aria-label="Toggle vibration"
                  >
                    <span />
                  </button>
                </div>

                <div className="settings-row settings-select-row">
                  <div className="settings-row-info">
                    <div className="settings-row-icon green">
                      <Clock3 size={18} />
                    </div>

                    <div>
                      <h3>Advance Reminder Time</h3>

                      <p>Notify you before the actual medicine time.</p>
                    </div>
                  </div>

                  <select
                    name="reminderTime"
                    value={settings.reminderTime}
                    onChange={handleChange}
                    className="settings-select"
                  >
                    <option value="0">At scheduled time</option>

                    <option value="5">5 minutes before</option>

                    <option value="10">10 minutes before</option>

                    <option value="15">15 minutes before</option>

                    <option value="30">30 minutes before</option>
                  </select>
                </div>
              </div>
            </div>

            {/* APPEARANCE */}

            <div className="settings-card">
              <div className="settings-card-heading">
                <div className="settings-heading-icon purple">
                  <Moon size={21} />
                </div>

                <div>
                  <h2>Appearance</h2>

                  <p>Choose how MediReminder looks.</p>
                </div>
              </div>

              <div className="settings-theme-options">
                <button
                  type="button"
                  className={`settings-theme-card ${
                    settings.appearance === "light" ? "active" : ""
                  }`}
                  onClick={() =>
                    handleAppearanceChange("light")
                  }
                >
                  <div className="settings-theme-preview light">
                    <div />
                    <div />
                    <div />
                  </div>

                  <span>Light</span>
                </button>

                <button
                  type="button"
                  className={`settings-theme-card ${
                    settings.appearance === "dark" ? "active" : ""
                  }`}
                  onClick={() =>
                    handleAppearanceChange("dark")
                  }
                >
                  <div className="settings-theme-preview dark">
                    <div />
                    <div />
                    <div />
                  </div>

                  <span>Dark</span>
                </button>

                <button
                  type="button"
                  className={`settings-theme-card ${
                    settings.appearance === "system" ? "active" : ""
                  }`}
                  onClick={() =>
                    handleAppearanceChange("system")
                  }
                >
                  <div className="settings-theme-preview system">
                    <div className="settings-preview-left" />

                    <div className="settings-preview-right" />
                  </div>

                  <span>System</span>
                </button>
              </div>
            </div>
          </div>

          {/* RIGHT COLUMN */}

          <aside className="settings-side-column">
            {/* SECURITY */}

            <div className="settings-side-card">
              <div className="settings-side-heading">
                <ShieldCheck size={20} />

                <h2>Privacy & Security</h2>
              </div>

              <button type="button" className="settings-action-row" onClick={handleChangePassword}>
                <div className="settings-action-left">
                  <div className="settings-action-icon">
                    <LockKeyhole size={17} />
                  </div>

                  <div>
                    <strong>Change Password</strong>

                    <span>Update your account password</span>
                  </div>
                </div>

                <ChevronRight size={17} />
              </button>

              <button type="button" className="settings-action-row" onClick={() => setMessage("Privacy preferences are managed through the notification and account settings on this page.")}>
                <div className="settings-action-left">
                  <div className="settings-action-icon">
                    <ShieldCheck size={17} />
                  </div>

                  <div>
                    <strong>Privacy Settings</strong>

                    <span>Manage your health data privacy</span>
                  </div>
                </div>

                <ChevronRight size={17} />
              </button>
            </div>

            {/* ACCOUNT */}

            <div className="settings-side-card">
              <div className="settings-side-heading">
                <Smartphone size={20} />

                <h2>Account</h2>
              </div>

              <button type="button" className="settings-action-row" onClick={() => { clearSession(); navigate("/login"); }}>
                <div className="settings-action-left">
                  <div className="settings-action-icon logout">
                    <LogOut size={17} />
                  </div>

                  <div>
                    <strong>Log Out</strong>

                    <span>Sign out of your account</span>
                  </div>
                </div>

                <ChevronRight size={17} />
              </button>

              <button type="button" className="settings-action-row delete" onClick={handleDeleteAccount}>
                <div className="settings-action-left">
                  <div className="settings-action-icon delete">
                    <Trash2 size={17} />
                  </div>

                  <div>
                    <strong>Delete Account</strong>

                    <span>Permanently delete your account</span>
                  </div>
                </div>

                <ChevronRight size={17} />
              </button>
            </div>

            {/* INFORMATION CARD */}

            <div className="settings-info-card">
              <div className="settings-info-icon">
                <Bell size={20} />
              </div>

              <div>
                <h3>Stay on track</h3>

                <p>
                  Keeping reminders enabled helps you stay consistent with your
                  medicine schedule.
                </p>
              </div>
            </div>
          </aside>
        </section>
      </main>
    </div>
  );
}

export default Settings;
