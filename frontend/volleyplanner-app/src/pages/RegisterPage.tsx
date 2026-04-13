import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AxiosError } from "axios";
import { authService } from "../services/authService";
import "../styles/auth.css";

function RegisterPage() {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    name: "",
    email: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");
    setLoading(true);

    try {
      await authService.register(formData);
      setSuccessMessage("Sikeres regisztráció! Átirányítás bejelentkezéshez...");
      setTimeout(() => navigate("/"), 1200);
    } catch (err: unknown) {
      if (err instanceof AxiosError) {
        setError(err.response?.data?.message || "Sikertelen regisztráció.");
      } else {
        setError("Sikertelen regisztráció.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <h1>VolleyMind</h1>
          <p>Hozd létre a fiókodat, és kezdd el az edzéstervezést</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="auth-field">
            <label htmlFor="name">Felhasználónév</label>
            <input
              id="name"
              type="text"
              name="name"
              placeholder="Add meg a neved"
              value={formData.name}
              onChange={handleChange}
              required
            />
          </div>

          <div className="auth-field">
            <label htmlFor="email">Email cím</label>
            <input
              id="email"
              type="email"
              name="email"
              placeholder="pelda@email.com"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="auth-field">
            <label htmlFor="password">Jelszó</label>
            <input
              id="password"
              type="password"
              name="password"
              placeholder="Adj meg egy biztonságos jelszót"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>

          {error && <div className="auth-error">{error}</div>}
          {successMessage && <div className="auth-success">{successMessage}</div>}

          <button className="auth-button" type="submit" disabled={loading}>
            {loading ? "Regisztráció..." : "Regisztráció"}
          </button>
        </form>

        <div className="auth-footer">
          Van már fiókod? <Link to="/">Bejelentkezés</Link>
        </div>
      </div>
    </div>
  );
}

export default RegisterPage;