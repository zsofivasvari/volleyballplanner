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

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    setError("");
    setSuccessMessage("");
    setLoading(true);

    try {
      await authService.register(formData);

      setSuccessMessage(
        "Sikeres regisztráció. Ellenőrizd az email fiókodat a megerősítő linkért."
      );

      setTimeout(() => {
        navigate("/");
      }, 2500);
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
    <main
      className="auth-page auth-page-image"
      style={{ backgroundImage: "url('/images/login.png')" }}
    >
      <section className="auth-card">
        <h1>VolleyMind</h1>
        <p className="auth-subtitle">
          Hozz létre fiókot az edzéstervező és fejlődéskövető rendszerhez.
        </p>

        <form onSubmit={handleSubmit}>
          <div className="auth-field">
            <label htmlFor="name">Név</label>
            <input
              id="name"
              name="name"
              type="text"
              placeholder="Teljes név"
              value={formData.name}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="auth-field">
            <label htmlFor="email">Email cím</label>
            <input
              id="email"
              name="email"
              type="email"
              placeholder="pelda@email.com"
              value={formData.email}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="auth-field">
            <label htmlFor="password">Jelszó</label>
            <input
              id="password"
              name="password"
              type="password"
              placeholder="••••••••"
              value={formData.password}
              onChange={handleInputChange}
              required
            />
          </div>

          {error && <p className="auth-error">{error}</p>}
          {successMessage && <p className="auth-success">{successMessage}</p>}

          <button className="auth-button" type="submit" disabled={loading}>
            {loading ? "Regisztráció..." : "Regisztráció"}
          </button>
        </form>

        <p className="auth-bottom-text">
          Van már fiókod? <Link to="/">Bejelentkezés</Link>
        </p>
      </section>
    </main>
  );
}

export default RegisterPage;