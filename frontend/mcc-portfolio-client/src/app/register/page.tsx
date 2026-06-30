"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, User, Mail, Lock, Key, ShieldAlert } from "lucide-react";
import api from "@/services/api";

export default function RegisterPage() {
  const router = useRouter();

  const aidedDepartments = [
    "English", "Tamil", "Languages", "History", "Political Science", "Public Administration",
    "Economics", "Philosophy", "Commerce", "Social Work", "Mathematics", "Statistics",
    "Physics", "Chemistry", "Botany", "Zoology"
  ];

  const sfsDepartments = [
    "English", "Tamil", "Languages", "Journalism", "Social Work", "Commerce",
    "Business Administration", "Communication", "Geography", "Tourism Studies",
    "Mathematics", "Physics", "Chemistry", "Microbiology", "Computer Application (BCA)",
    "Computer Science (B.Sc)", "Computer Science (MCA)", "Visual Communication",
    "Physical Education, Health Education and Sports", "Psychology", "Data Science", "Physical Education"
  ];

  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [stream, setStream] = useState("");
  const [department, setDepartment] = useState("");
  const [registerNumber, setRegisterNumber] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [showSocialModal, setShowSocialModal] = useState(false);
  const [socialProvider, setSocialProvider] = useState("");
  const [socialEmail, setSocialEmail] = useState("");
  const [socialName, setSocialName] = useState("");

  const handleStreamChange = (val: string) => {
    setStream(val);
    setDepartment("");
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!fullName || !email || !password || !registerNumber) {
      setError("All fields are required.");
      return;
    }

    if (!stream) {
      setError("Stream is required.");
      return;
    }

    if (!department) {
      setError("Department is required.");
      return;
    }

    // Double check that department belongs to stream
    const validDepts = stream === "Aided" ? aidedDepartments : sfsDepartments;
    if (!validDepts.includes(department)) {
      setError("The selected department does not match the chosen stream.");
      return;
    }

    try {
      setLoading(true);
      setError("");

      const response = await api.post("/Auth/register", {
        fullName,
        email,
        password,
        stream,
        department,
        registerNumber,
      });

      localStorage.setItem("token", response.data.token);
      localStorage.setItem("user", JSON.stringify(response.data));
      router.push("/dashboard");
    } catch (err: any) {
      setError(err.response?.data || "Registration failed");
    } finally {
      setLoading(false);
    }
  };

  const handleExternalSignIn = (provider: string) => {
    setSocialProvider(provider);
    if (provider === "Google") {
      setSocialEmail("alwin.rosh@gmail.com");
      setSocialName("Alwin Rosh");
    } else {
      setSocialEmail("alwinrosh_git@mcc.edu");
      setSocialName("Alwin Rosh GitHub");
    }
    setShowSocialModal(true);
  };

  const submitExternalLogin = async () => {
    if (!socialEmail || !socialName) {
      alert("Please enter both email and name.");
      return;
    }
    try {
      setLoading(true);
      setError("");
      const response = await api.post("/Auth/external-login", {
        email: socialEmail,
        fullName: socialName,
        provider: socialProvider,
        externalId: "ext_" + Math.random().toString(36).substring(2, 9),
      });

      localStorage.setItem("token", response.data.token);
      localStorage.setItem("user", JSON.stringify(response.data));
      router.push("/dashboard");
    } catch {
      setError("External registration simulation failed");
    } finally {
      setLoading(false);
      setShowSocialModal(false);
    }
  };

  return (
    <div className="min-h-screen flex bg-[#fcfaf6] text-[#2c2c2c] font-sans">
      
      {/* LEFT PANEL: CAMPUS ARCHWAY SHOWCASE (Hidden on Mobile) */}
      <div className="hidden lg:flex lg:w-5/12 relative bg-[#18233c] text-white p-12 flex-col justify-between overflow-hidden">
        <div className="absolute inset-0 z-0">
          <img 
            src="/mcc-entrance-gate.jpg" 
            alt="MCC Entrance Gate" 
            className="w-full h-full object-cover opacity-35 filter brightness-75 contrast-125"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-[#18233c] via-[#18233c]/60 to-transparent" />
        </div>

        <div className="relative z-10 flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-[#781c1c] flex items-center justify-center shadow-md">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="#f7f5f0" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" className="w-5 h-5">
              <circle cx="12" cy="5" r="3" />
              <line x1="12" y1="8" x2="12" y2="22" />
              <line x1="6" y1="12" x2="18" y2="12" />
              <path d="M5 12a7 7 0 0 0 14 0" />
            </svg>
          </div>
          <div>
            <span className="font-serif font-black text-sm tracking-wider block uppercase">Madras Christian College</span>
            <span className="text-[8px] uppercase tracking-widest text-amber-400 block font-bold">Autonomous placement directory</span>
          </div>
        </div>

        <div className="relative z-10 max-w-sm mb-8">
          <h2 className="font-serif text-3xl font-extrabold text-white leading-tight mb-4">
            Join the Verified Registry
          </h2>
          <p className="text-xs text-slate-350 leading-relaxed">
            Create an official student resume profile. Showcase academic records, projects, research papers, languages, patents, test scores, and handles directly to verifying administrators.
          </p>
        </div>

        <div className="relative z-10 text-[10px] font-mono text-slate-400 uppercase tracking-widest">
          Established 1837 · Chennai, India
        </div>
      </div>

      {/* RIGHT PANEL: REGISTER FORM SECTION */}
      <div className="w-full lg:w-7/12 flex items-center justify-center p-6 md:p-12 overflow-y-auto">
        <div className="w-full max-w-xl space-y-6">
          
          <Link href="/" className="inline-flex items-center gap-2 text-xs font-bold text-slate-500 hover:text-[#781c1c] transition duration-200">
            <ArrowLeft size={14} /> Back to Home
          </Link>

          <div className="bg-white border border-slate-200 rounded-3xl p-8 md:p-10 shadow-lg">
            
            <div className="text-center mb-8">
              <h1 className="font-serif text-3xl font-extrabold text-[#18233c] mb-1">
                Student Registration
              </h1>
              <p className="text-xs text-slate-500 font-medium">
                Create your verified placement profile on the platform
              </p>
            </div>

            <form onSubmit={handleRegister} className="space-y-4">
              
              <div className="grid md:grid-cols-2 gap-4">
                
                <div>
                  <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                    Full Name
                  </label>
                  <div className="relative">
                    <User className="absolute left-4 top-3.5 text-slate-400" size={16} />
                    <input
                      type="text"
                      required
                      placeholder="Enter full name"
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 placeholder-slate-400 text-xs px-11 py-3.5 rounded-xl outline-none focus:ring-1 focus:ring-[#781c1c]/10 transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                    Email Address
                  </label>
                  <div className="relative">
                    <Mail className="absolute left-4 top-3.5 text-slate-400" size={16} />
                    <input
                      type="email"
                      required
                      placeholder="Enter college email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 placeholder-slate-400 text-xs px-11 py-3.5 rounded-xl outline-none focus:ring-1 focus:ring-[#781c1c]/10 transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                    Password
                  </label>
                  <div className="relative">
                    <Lock className="absolute left-4 top-3.5 text-slate-400" size={16} />
                    <input
                      type="password"
                      required
                      placeholder="Min 6 characters"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 placeholder-slate-400 text-xs px-11 py-3.5 rounded-xl outline-none focus:ring-1 focus:ring-[#781c1c]/10 transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                    Register Number
                  </label>
                  <div className="relative">
                    <Key className="absolute left-4 top-3.5 text-slate-400" size={16} />
                    <input
                      type="text"
                      required
                      placeholder="e.g. 2111711019011"
                      value={registerNumber}
                      onChange={(e) => setRegisterNumber(e.target.value)}
                      className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 placeholder-slate-400 text-xs px-11 py-3.5 rounded-xl outline-none focus:ring-1 focus:ring-[#781c1c]/10 transition"
                    />
                  </div>
                </div>

                <div>
                  <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                    Stream *
                  </label>
                  <select
                    required
                    value={stream}
                    onChange={(e) => handleStreamChange(e.target.value)}
                    className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 text-xs px-4 py-3.5 rounded-xl outline-none transition cursor-pointer"
                  >
                    <option value="">Select Stream</option>
                    <option value="Aided">Aided</option>
                    <option value="SFS">SFS</option>
                  </select>
                </div>

                <div>
                  <label className="text-[10px] uppercase font-mono tracking-wider font-bold text-slate-650 block mb-1.5">
                    Department *
                  </label>
                  <select
                    required
                    disabled={!stream}
                    value={department}
                    onChange={(e) => setDepartment(e.target.value)}
                    className="w-full bg-slate-50 border border-slate-200 focus:border-[#781c1c] text-slate-800 text-xs px-4 py-3.5 rounded-xl outline-none transition disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer"
                  >
                    <option value="">Select Department</option>
                    {stream === "Aided" && aidedDepartments.map((dept, idx) => (
                      <option key={idx} value={dept}>{dept}</option>
                    ))}
                    {stream === "SFS" && sfsDepartments.map((dept, idx) => (
                      <option key={idx} value={dept}>{dept}</option>
                    ))}
                  </select>
                </div>

              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 text-xs p-3 rounded-xl flex items-center gap-2">
                  <ShieldAlert size={14} className="shrink-0" />
                  <span>{error}</span>
                </div>
              )}

              <button
                type="submit"
                disabled={loading}
                className="w-full py-3.5 rounded-xl bg-[#781c1c] hover:bg-[#5f1515] text-white font-bold text-xs uppercase tracking-wider transition-all duration-300 shadow-md cursor-pointer hover:shadow-lg active:scale-98"
              >
                {loading ? "Creating Account..." : "Register"}
              </button>

            </form>

            {/* Social Divider */}
            <div className="flex items-center my-6">
              <div className="flex-1 h-px bg-slate-200" />
              <span className="px-3 text-[10px] text-slate-400 uppercase tracking-widest font-mono font-bold">Or Continue With</span>
              <div className="flex-1 h-px bg-slate-200" />
            </div>

            {/* Social Logins */}
            <div className="grid grid-cols-2 gap-3 max-w-md mx-auto">
              <button
                onClick={() => handleExternalSignIn("Google")}
                type="button"
                className="py-3 px-4 rounded-xl border border-slate-200 hover:border-slate-350 hover:bg-slate-50 text-slate-700 text-[11px] font-bold transition flex items-center justify-center gap-2 cursor-pointer hover:scale-[1.01]"
              >
                Google
              </button>
              <button
                onClick={() => handleExternalSignIn("GitHub")}
                type="button"
                className="py-3 px-4 rounded-xl border border-slate-200 hover:border-slate-350 hover:bg-slate-50 text-slate-700 text-[11px] font-bold transition flex items-center justify-center gap-2 cursor-pointer hover:scale-[1.01]"
              >
                GitHub
              </button>
            </div>

            <p className="text-center text-slate-400 mt-6 text-[10px] uppercase font-mono font-semibold tracking-wider">
              Join fellow student leaders
            </p>

          </div>

          <div className="text-center text-xs text-slate-500 pb-4">
            Already have an account? <Link href="/login" className="text-[#781c1c] font-bold hover:underline">Log in here</Link>
          </div>

        </div>
      </div>

      {/* Simulated Social OAuth Dialog */}
      {showSocialModal && (
        <div className="fixed inset-0 z-50 bg-[#18233c]/60 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="relative w-full max-w-md bg-white border border-slate-200 rounded-3xl p-8 shadow-2xl text-left">
            <h3 className="text-lg font-serif font-extrabold text-[#18233c] mb-2 flex items-center gap-2">
              🔒 Simulated {socialProvider} Authentication
            </h3>
            <p className="text-slate-500 text-xs mb-6">
              This simulated social OAuth registers you and issues a standard JWT token locally.
            </p>

            <div className="space-y-4">
              <div>
                <label className="text-[10px] uppercase font-mono font-bold text-slate-650 block mb-1">Full Name</label>
                <input
                  type="text"
                  value={socialName}
                  onChange={(e) => setSocialName(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-xs text-slate-800 focus:outline-none focus:border-[#781c1c]"
                />
              </div>

              <div>
                <label className="text-[10px] uppercase font-mono font-bold text-slate-650 block mb-1">Social Email Address</label>
                <input
                  type="email"
                  value={socialEmail}
                  onChange={(e) => setSocialEmail(e.target.value)}
                  className="w-full bg-slate-50 border border-slate-200 rounded-xl px-4 py-2.5 text-xs text-slate-800 focus:outline-none focus:border-[#781c1c]"
                />
              </div>

              <div className="pt-2 flex gap-3">
                <button
                  onClick={() => setShowSocialModal(false)}
                  className="flex-1 py-2.5 rounded-xl border border-slate-200 hover:bg-slate-50 text-xs font-bold text-slate-650 transition"
                >
                  Cancel
                </button>
                <button
                  onClick={submitExternalLogin}
                  className="flex-1 py-2.5 rounded-xl bg-[#781c1c] text-white text-xs font-bold transition hover:opacity-95"
                >
                  Confirm & Sign In
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}