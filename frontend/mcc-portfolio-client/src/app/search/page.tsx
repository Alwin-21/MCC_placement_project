"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import {
  Search,
  User,
  GraduationCap,
  ArrowRight,
  ArrowLeft,
  Mail,
  MapPin
} from "lucide-react";
import api from "@/services/api";

export default function SearchPage() {
  const [query, setQuery] = useState("");
  const [students, setStudents] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  const searchStudents = async (searchQuery = query) => {
    try {
      setLoading(true);
      const response = await api.get(`/Search?query=${searchQuery}`);
      setStudents(response.data);
    } catch (error) {
      console.error("Search query execution failed", error);
    } finally {
      setLoading(false);
    }
  };

  // Pre-fetch some initial students or empty state suggestions
  useEffect(() => {
    searchStudents("");
  }, []);

  const handleQuickSearch = (tag: string) => {
    setQuery(tag);
    searchStudents(tag);
  };

  return (
    <div className="min-h-screen bg-[#fcfaf6] text-[#2c2c2c] selection:bg-[#781c1c]/10 selection:text-[#781c1c] pb-20 font-sans">
      
      {/* TOP NAVBAR */}
      <div className="border-b border-[#781c1c]/10 bg-white/90 backdrop-blur-md sticky top-0 z-50 transition-all duration-300 shadow-xs">
        <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
          <Link href="/" className="inline-flex items-center gap-2 text-xs font-bold text-slate-655 hover:text-[#781c1c] transition duration-200">
            <ArrowLeft size={14} /> Back to Home
          </Link>
          
          <div className="flex items-center gap-2">
            <span className="w-2.5 h-2.5 rounded-full bg-emerald-500 animate-pulse" />
            <span className="text-[10px] uppercase font-mono tracking-widest text-[#781c1c] font-black">
              MCC Student Directory
            </span>
          </div>
        </div>
      </div>

      <div className="max-w-6xl mx-auto px-6 pt-16 relative z-10">
        
        {/* TITLE SECTION */}
        <div className="text-center max-w-2xl mx-auto mb-12 space-y-3">
          <h4 className="text-xs font-black uppercase tracking-widest text-[#781c1c] font-mono">Talent Registry</h4>
          <h1 className="font-serif text-4xl md:text-5xl font-black text-[#18233c] tracking-tight leading-tight">
            Discover MCC Talent
          </h1>
          <p className="text-sm text-slate-500 leading-relaxed">
            Search our comprehensive directory of verified Madras Christian College student profiles by name, technical skill, or academic major.
          </p>
        </div>

        {/* SEARCH BAR CARD */}
        <div className="bg-white border border-slate-200 p-6 rounded-[28px] shadow-md mb-10">
          <div className="flex flex-col sm:flex-row gap-3">
            <div className="flex-1 bg-slate-50 border border-slate-200 rounded-2xl px-5 py-3.5 flex items-center gap-3 focus-within:border-[#781c1c] focus-within:ring-1 focus-within:ring-[#781c1c]/10 transition-all">
              <Search className="text-slate-400 flex-shrink-0" size={18} />
              <input
                type="text"
                placeholder="Search by name, skill (e.g. React, C#), or department..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyDown={(e) => e.key === "Enter" && searchStudents()}
                className="bg-transparent border-none outline-none text-xs text-slate-800 placeholder-slate-400 w-full"
              />
            </div>
            
            <button
              onClick={() => searchStudents()}
              disabled={loading}
              className="bg-[#781c1c] hover:bg-[#5f1515] text-white font-bold px-8 py-3.5 rounded-2xl text-xs uppercase tracking-wider transition duration-200 flex-shrink-0 active:scale-[0.98] shadow-sm hover:shadow"
            >
              {loading ? "Searching Directory..." : "Search Profiles"}
            </button>
          </div>

          {/* QUICK TARGET TAGS */}
          <div className="mt-4 flex items-center gap-2 flex-wrap text-[11px]">
            <span className="text-slate-500 font-semibold mr-1">Popular Skill Tags:</span>
            {["React", "C#", "Next.js", "Python", "SQL", "Tailwind", "Machine Learning"].map((tag) => (
              <button
                key={tag}
                onClick={() => handleQuickSearch(tag)}
                className="px-3.5 py-1.5 rounded-xl bg-slate-50 hover:bg-amber-50 hover:text-[#781c1c] hover:border-amber-600/30 text-slate-600 border border-slate-200 transition duration-200 font-medium"
              >
                {tag}
              </button>
            ))}
          </div>
        </div>

        {/* RESULTS GRID */}
        {loading ? (
          <div className="py-20 text-center">
            <div className="w-12 h-12 border-4 border-t-[#781c1c] border-r-amber-500 border-b-transparent border-l-transparent rounded-full animate-spin mx-auto mb-4" />
            <p className="text-slate-500 text-xs font-bold tracking-wider uppercase animate-pulse">Filtering MCC Profiles...</p>
          </div>
        ) : students.length > 0 ? (
          <div className="space-y-6">
            <div className="flex justify-between items-center text-[10px] font-mono font-bold tracking-wider text-slate-500 mb-2 px-1">
              <span>FOUND {students.length} STUDENT MATCHES</span>
              <span>VERIFIED DIRECTORY PROFILES</span>
            </div>

            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
              {students.map((student) => {
                const initials = student.fullName
                  ? student.fullName.split(" ").map((n: string) => n[0]).join("").slice(0, 2).toUpperCase()
                  : "ST";

                return (
                  <div
                    key={student.id}
                    className="bg-white border border-slate-200 hover:border-amber-600/30 rounded-3xl p-6 relative group transition-all duration-300 flex flex-col justify-between hover:shadow-lg"
                  >
                    <div>
                      {/* HEADER ROW */}
                      <div className="flex items-center gap-4 mb-4">
                        <div className="w-12 h-12 rounded-xl bg-[#781c1c]/5 border border-[#781c1c]/10 flex items-center justify-center font-bold text-[#781c1c] shrink-0 text-sm">
                          {initials}
                        </div>
                        <div className="min-w-0">
                          <h3 className="text-base font-bold text-[#18233c] group-hover:text-[#781c1c] transition-colors truncate">
                            {student.fullName}
                          </h3>
                          <span className="text-[9px] uppercase font-mono tracking-widest text-[#781c1c] font-black">
                            {student.department || "Madras Christian College"}
                          </span>
                        </div>
                      </div>

                      <div className="space-y-1.5 text-xs text-slate-500 mb-6">
                        <p className="truncate flex items-center gap-1.5"><Mail size={12} className="text-slate-400 shrink-0" /> {student.email}</p>
                        {student.currentLocation && (
                          <p className="truncate flex items-center gap-1.5"><MapPin size={12} className="text-slate-400 shrink-0" /> {student.currentLocation}</p>
                        )}
                      </div>
                    </div>

                    <div className="flex items-center justify-between border-t border-slate-100 pt-4 mt-2">
                      <span className="inline-flex items-center gap-1 text-[9px] font-bold text-emerald-700 bg-emerald-50 border border-emerald-250 px-2.5 py-0.5 rounded-full uppercase">
                        ✓ VERIFIED
                      </span>

                      <Link
                        href={`/portfolio/${student.id}`}
                        target="_blank"
                        className="inline-flex items-center gap-1.5 text-xs text-[#781c1c] hover:text-[#5f1515] font-bold"
                      >
                        View Portfolio
                        <ArrowRight size={14} className="group-hover:translate-x-0.5 transition-transform" />
                      </Link>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        ) : (
          <div className="text-center py-20 bg-white border border-slate-200 rounded-[32px] px-6 shadow-sm">
            <User size={48} className="text-slate-350 mx-auto mb-4" />
            <h3 className="font-serif text-xl font-bold text-[#18233c] mb-2">No Profiles Found</h3>
            <p className="text-slate-500 text-xs max-w-sm mx-auto leading-relaxed">
              No verified student portfolio matches your keyword. Try looking up tags like "React" or "C#".
            </p>
          </div>
        )}

      </div>
    </div>
  );
}