import { BrowserRouter, Routes, Route } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ExercisesPage from "./pages/ExercisesPage";
import ExerciseDetailsPage from "./pages/ExerciseDetailsPage";
import CreateExercisePage from "./pages/CreateExercisePage";
import GenerateTrainingPlanPage from "./pages/GenerateTrainingPlanPage";
import ProtectedRoute from "./routes/ProtectedRoute";
import SavedPlansPage from "./pages/SavedPlansPage";
import TrainingPlanDetailsPage from "./pages/TrainingPlanDetailsPage";
import WeeklyPlannerPage from "./pages/WeeklyPlannerPage";
import ConfirmEmailPage from "./pages/ConfirmEmailPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
import StatisticsPage from "./pages/StatisticsPage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        <Route
          path="/exercises"
          element={
            <ProtectedRoute>
              <ExercisesPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/exercises/new"
          element={
            <ProtectedRoute>
              <CreateExercisePage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/exercises/:id"
          element={
            <ProtectedRoute>
              <ExerciseDetailsPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/generate"
          element={
            <ProtectedRoute>
              <GenerateTrainingPlanPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/plans"
          element={
            <ProtectedRoute>
              <SavedPlansPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/plans/:id"
          element={
            <ProtectedRoute>
              <TrainingPlanDetailsPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/planner"
          element={
            <ProtectedRoute>
              <WeeklyPlannerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/statistics"
          element={
            <ProtectedRoute>
              <StatisticsPage />
            </ProtectedRoute>
          }
        />

        <Route path="/confirm-email" element={<ConfirmEmailPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;