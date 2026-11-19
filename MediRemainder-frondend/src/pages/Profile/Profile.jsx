import { useEffect, useState } from "react";

import {
  Camera,
  Edit3,
  HeartPulse,
  Mail,
  Phone,
  Save,
  ShieldCheck,
  User,
  X,
} from "lucide-react";

import Sidebar from "../../components/Sidebar/Sidebar";
import "./Profile.css";
import { errorMessage, updateStoredUser } from "../../api/client";
import { profileApi } from "../../api/services";

const EMPTY_PROFILE = { fullName: "", email: "", mobile: "", dateOfBirth: "", gender: "", bloodGroup: "", emergencyName: "", emergencyMobile: "" };

function Profile() {
  const [isEditing, setIsEditing] = useState(false);

  const [profile, setProfile] = useState(EMPTY_PROFILE);

  const [formData, setFormData] = useState(profile);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [pageMessage, setPageMessage] = useState("");
  const [pageError, setPageError] = useState("");
  const completion = Math.round([
    profile.fullName, profile.email || profile.mobile, profile.dateOfBirth,
    profile.gender, profile.bloodGroup, profile.emergencyName, profile.emergencyMobile,
  ].filter(Boolean).length / 7 * 100);

  useEffect(() => {
    profileApi.get().then((result) => {
      const next = { ...EMPTY_PROFILE, ...result, mobile: result.mobileNumber || "" };
      setProfile(next);
      setFormData(next);
      updateStoredUser({ id: result.id, fullName: result.fullName, email: result.email, mobileNumber: result.mobileNumber });
    }).catch((error) => setPageError(errorMessage(error, "Could not load your profile.")))
      .finally(() => setLoading(false));
  }, []);

  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const handleEdit = () => {
    setFormData(profile);
    setIsEditing(true);
  };

  const handleCancel = () => {
    setFormData(profile);
    setIsEditing(false);
  };

  const handleSave = async () => {
    setSaving(true); setPageError(""); setPageMessage("");
    try {
      const result = await profileApi.update({
        fullName: formData.fullName, email: formData.email || null,
        mobileNumber: formData.mobile || null, dateOfBirth: formData.dateOfBirth || null,
        gender: formData.gender || null, bloodGroup: formData.bloodGroup || null,
        emergencyName: formData.emergencyName || null, emergencyMobile: formData.emergencyMobile || null,
      });
      const next = { ...EMPTY_PROFILE, ...result, mobile: result.mobileNumber || "" };
      setProfile(next); setFormData(next);
      updateStoredUser({ id: result.id, fullName: result.fullName, email: result.email, mobileNumber: result.mobileNumber });
      setIsEditing(false); setPageMessage("Profile updated successfully.");
    } catch (error) { setPageError(errorMessage(error, "Could not save your profile.")); }
    finally { setSaving(false); }
  };

  return (
    <div className="profile-page-layout">
      <Sidebar activePage="Profile" />

      <main className="profile-page-main">
        {/* HEADER */}

        <section className="profile-page-header">
          <div>
            <p className="profile-breadcrumb">Profile</p>

            <h1>My Profile</h1>

            <p className="profile-subtitle">
              Manage your personal and health information.
            </p>
          </div>

          {!isEditing ? (
            <button
              type="button"
              className="profile-edit-button"
              onClick={handleEdit}
            >
              <Edit3 size={17} />
              Edit Profile
            </button>
          ) : (
            <div className="profile-header-actions">
              <button
                type="button"
                className="profile-cancel-button"
                onClick={handleCancel}
              >
                <X size={16} />
                Cancel
              </button>

              <button
                type="button"
                className="profile-save-button"
                onClick={handleSave}
                disabled={saving}
              >
                <Save size={16} />
                {saving ? "Saving..." : "Save Changes"}
              </button>
            </div>
          )}
        </section>

        {(loading || pageError || pageMessage) && (
          <p role={pageError ? "alert" : "status"}>
            {loading ? "Loading profile..." : pageError || pageMessage}
          </p>
        )}

        {/* MAIN GRID */}

        <section className="profile-content-grid">
          {/* LEFT SIDE */}

          <div className="profile-left-column">
            {/* PROFILE CARD */}

            <div className="profile-main-card">
              <div className="profile-avatar-section">
                <div className="profile-avatar-wrapper">
                  <div className="profile-avatar">
                    {profile.fullName
                      ? profile.fullName.charAt(0).toUpperCase()
                      : "U"}
                  </div>

                  <button
                    type="button"
                    className="profile-camera-button"
                    aria-label="Change profile picture"
                  >
                    <Camera size={15} />
                  </button>
                </div>

                <div className="profile-avatar-info">
                  <h2>{profile.fullName}</h2>

                  <p>Keep your health details updated</p>

                  <span className="profile-status-badge">
                    <ShieldCheck size={14} />
                    Profile Active
                  </span>
                </div>
              </div>
            </div>

            {/* PERSONAL INFORMATION */}

            <div className="profile-info-card">
              <div className="profile-section-title">
                <div className="profile-section-icon">
                  <User size={20} />
                </div>

                <div>
                  <h2>Personal Information</h2>

                  <p>Basic details about your profile</p>
                </div>
              </div>

              <div className="profile-form-grid">
                {/* NAME */}

                <div className="profile-form-group">
                  <label>Full Name</label>

                  {isEditing ? (
                    <input
                      type="text"
                      name="fullName"
                      value={formData.fullName}
                      onChange={handleChange}
                    />
                  ) : (
                    <div className="profile-display-field">
                      {profile.fullName}
                    </div>
                  )}
                </div>

                {/* DOB */}

                <div className="profile-form-group">
                  <label>Date of Birth</label>

                  {isEditing ? (
                    <input
                      type="date"
                      name="dateOfBirth"
                      value={formData.dateOfBirth}
                      onChange={handleChange}
                    />
                  ) : (
                    <div className="profile-display-field">
                      {profile.dateOfBirth || "Not added"}
                    </div>
                  )}
                </div>

                {/* EMAIL */}

                <div className="profile-form-group">
                  <label>Email Address</label>

                  {isEditing ? (
                    <div className="profile-input-with-icon">
                      <Mail size={16} />

                      <input
                        type="email"
                        name="email"
                        value={formData.email}
                        onChange={handleChange}
                      />
                    </div>
                  ) : (
                    <div className="profile-display-field profile-field-with-icon">
                      <Mail size={16} />
                      {profile.email}
                    </div>
                  )}
                </div>

                {/* PHONE */}

                <div className="profile-form-group">
                  <label>Mobile Number</label>

                  {isEditing ? (
                    <div className="profile-input-with-icon">
                      <Phone size={16} />

                      <input
                        type="tel"
                        name="mobile"
                        value={formData.mobile}
                        onChange={handleChange}
                      />
                    </div>
                  ) : (
                    <div className="profile-display-field profile-field-with-icon">
                      <Phone size={16} />
                      {profile.mobile}
                    </div>
                  )}
                </div>

                {/* GENDER */}

                <div className="profile-form-group">
                  <label>Gender</label>

                  {isEditing ? (
                    <select
                      name="gender"
                      value={formData.gender}
                      onChange={handleChange}
                    >
                      <option value="">Select gender</option>
                      <option value="Female">Female</option>
                      <option value="Male">Male</option>
                      <option value="Other">Other</option>
                      <option value="Prefer not to say">
                        Prefer not to say
                      </option>
                    </select>
                  ) : (
                    <div className="profile-display-field">
                      {profile.gender || "Not added"}
                    </div>
                  )}
                </div>

                {/* BLOOD GROUP */}

                <div className="profile-form-group">
                  <label>Blood Group</label>

                  {isEditing ? (
                    <select
                      name="bloodGroup"
                      value={formData.bloodGroup}
                      onChange={handleChange}
                    >
                      <option value="">Select blood group</option>
                      <option value="A+">A+</option>
                      <option value="A-">A-</option>
                      <option value="B+">B+</option>
                      <option value="B-">B-</option>
                      <option value="AB+">AB+</option>
                      <option value="AB-">AB-</option>
                      <option value="O+">O+</option>
                      <option value="O-">O-</option>
                    </select>
                  ) : (
                    <div className="profile-display-field">
                      {profile.bloodGroup || "Not added"}
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* EMERGENCY CONTACT */}

            <div className="profile-info-card">
              <div className="profile-section-title">
                <div className="profile-section-icon emergency">
                  <HeartPulse size={20} />
                </div>

                <div>
                  <h2>Emergency Contact</h2>

                  <p>Person to contact during an emergency</p>
                </div>
              </div>

              <div className="profile-form-grid">
                <div className="profile-form-group">
                  <label>Contact Name</label>

                  {isEditing ? (
                    <input
                      type="text"
                      name="emergencyName"
                      value={formData.emergencyName}
                      onChange={handleChange}
                    />
                  ) : (
                    <div className="profile-display-field">
                      {profile.emergencyName || "Not added"}
                    </div>
                  )}
                </div>

                <div className="profile-form-group">
                  <label>Contact Number</label>

                  {isEditing ? (
                    <div className="profile-input-with-icon">
                      <Phone size={16} />

                      <input
                        type="tel"
                        name="emergencyMobile"
                        value={formData.emergencyMobile}
                        onChange={handleChange}
                      />
                    </div>
                  ) : (
                    <div className="profile-display-field profile-field-with-icon">
                      <Phone size={16} />
                      {profile.emergencyMobile || "Not added"}
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* RIGHT SIDE */}

          <aside className="profile-right-column">
            {/* COMPLETION CARD */}

            <div className="profile-side-card profile-completion-card">
              <div className="profile-side-card-heading">
                <h3>Profile Completion</h3>

                <span>{completion}%</span>
              </div>

              <div className="profile-progress-track">
                <div className="profile-progress-value" style={{ width: `${completion}%` }} />
              </div>

              <p>
                Keeping your information updated helps organize your account details.
              </p>
            </div>

            {/* ACCOUNT SUMMARY */}

            <div className="profile-side-card">
              <div className="profile-side-title">
                <ShieldCheck size={19} />

                <h3>Account Summary</h3>
              </div>

              <div className="profile-summary-list">
                <div className="profile-summary-item">
                  <span>Account Status</span>

                  <strong className="profile-active-text">Active</strong>
                </div>

                <div className="profile-summary-item">
                  <span>Email</span>

                  <strong>{profile.email ? "On file" : "Not added"}</strong>
                </div>

                <div className="profile-summary-item">
                  <span>Mobile</span>

                  <strong>{profile.mobile ? "On file" : "Not added"}</strong>
                </div>

                <div className="profile-summary-item">
                  <span>Reminders</span>

                  <strong>Manage in Settings</strong>
                </div>
              </div>
            </div>

            {/* HEALTH NOTE */}

            <div className="profile-health-note">
              <div className="profile-health-note-icon">
                <HeartPulse size={22} />
              </div>

              <div>
                <h3>Your health matters</h3>

                <p>
                  Accurate profile information helps keep your medicine schedule
                  and reminders organized.
                </p>
              </div>
            </div>
          </aside>
        </section>
      </main>
    </div>
  );
}

export default Profile;
