-- ==============================================================================
-- PROJECT: NTC_OnlineFamily (Revit Cloud CMS)
-- DATE: 2025-12-16
-- DESCRIPTION: Database Schema Initialization for Supabase (PostgreSQL)
--              Adheres to DAMA-DMBOK for Governance & DDIA for Reliability.
-- ==============================================================================

-- 1. SETUP EXTENSIONS & TYPES
-- ==============================================================================

-- Enable pg_trgm for Fuzzy Search (Trigrams) on Family Names
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create Enumerated Types for Data Standardization
DO $$ BEGIN
    CREATE TYPE public.app_role AS ENUM ('admin', 'user');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    CREATE TYPE public.family_status AS ENUM ('pending', 'approved', 'rejected');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- 2. TABLE DEFINITIONS
-- ==============================================================================

-- Table: public.profiles
-- Description: Extends standard auth.users with app-specific roles
CREATE TABLE IF NOT EXISTS public.profiles (
    id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
    email TEXT,
    full_name TEXT,
    role public.app_role DEFAULT 'user'::public.app_role,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Secure the profiles table
ALTER TABLE public.profiles ENABLE ROW LEVEL SECURITY;

-- Trigger to automatically create profile on signup
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO public.profiles (id, email, full_name, role)
    VALUES (new.id, new.email, new.raw_user_meta_data->>'full_name', 'user');
    RETURN new;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Drop trigger if exists to ensure idempotency
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
    AFTER INSERT ON auth.users
    FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();

-- Table: public.families (The Core Asset)
CREATE TABLE IF NOT EXISTS public.families (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    name TEXT NOT NULL,
    category TEXT NOT NULL, -- e.g., 'Doors', 'Windows'
    revit_version INT NOT NULL CHECK (revit_version BETWEEN 2020 AND 2030),
    file_url TEXT NOT NULL, -- Link to Supabase Storage
    thumbnail_url TEXT,     -- Link to Preview Image
    parameters JSONB DEFAULT '{}'::JSONB, -- Revit Shared Parameters
    status public.family_status DEFAULT 'pending'::public.family_status,
    created_by UUID REFERENCES public.profiles(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),

    -- Constraints for Data Integrity
    CONSTRAINT unique_name_version UNIQUE (name, revit_version)
);

-- Secure the families table
ALTER TABLE public.families ENABLE ROW LEVEL SECURITY;

-- Table: public.downloads (Audit Trail)
-- Description: Tracks consumption metrics (Data Intelligence)
CREATE TABLE IF NOT EXISTS public.downloads (
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id UUID REFERENCES public.profiles(id) ON DELETE SET NULL,
    family_id UUID REFERENCES public.families(id) ON DELETE CASCADE,
    downloaded_at TIMESTAMPTZ DEFAULT NOW()
);

-- Secure the downloads table
ALTER TABLE public.downloads ENABLE ROW LEVEL SECURITY;

-- 3. ROW LEVEL SECURITY (RLS) POLICIES ("Defense in Depth")
-- ==============================================================================

-- Helper function to check if user is admin
CREATE OR REPLACE FUNCTION public.is_admin()
RETURNS BOOLEAN AS $$
BEGIN
  RETURN (SELECT role FROM public.profiles WHERE id = auth.uid()) = 'admin';
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- --- Policies for PROFILES ---
CREATE POLICY "Public profiles are viewable by everyone" ON public.profiles FOR SELECT USING (true);
CREATE POLICY "Users can update own profile" ON public.profiles FOR UPDATE USING (auth.uid() = id);

-- --- Policies for FAMILIES ---

-- SELECT: 
-- 1. Admins see everything.
-- 2. Owners see their own (even pending).
-- 3. Regular users see ONLY 'approved'.
CREATE POLICY "Families View Policy" ON public.families FOR SELECT
USING (
    status = 'approved' 
    OR auth.uid() = created_by 
    OR public.is_admin()
);

-- INSERT: Authenticated users can upload (Status defaults to 'pending')
CREATE POLICY "Families Insert Policy" ON public.families FOR INSERT
WITH CHECK (
    auth.role() = 'authenticated'
);

-- UPDATE:
-- 1. Admins can update everything (Approve/Reject).
-- 2. Owners can update ONLY if status is 'pending' (Fix metadata before approval).
CREATE POLICY "Families Update Policy" ON public.families FOR UPDATE
USING (
    public.is_admin() 
    OR (auth.uid() = created_by AND status = 'pending')
);

-- DELETE:
-- 1. Admins can delete everything.
-- 2. Owners can delete ONLY if status is 'pending'.
CREATE POLICY "Families Delete Policy" ON public.families FOR DELETE
USING (
    public.is_admin() 
    OR (auth.uid() = created_by AND status = 'pending')
);

-- --- Policies for DOWNLOADS ---

-- INSERT: Users can log their own downloads
CREATE POLICY "Downloads Insert Policy" ON public.downloads FOR INSERT
WITH CHECK (auth.uid() = user_id);

-- SELECT: Users can see their own history, Admins see all
CREATE POLICY "Downloads View Policy" ON public.downloads FOR SELECT
USING (auth.uid() = user_id OR public.is_admin());


-- 4. PERFORMANCE INDEXES (DDIA: Scalability)
-- ==============================================================================

-- Trigram Index for Fuzzy Search on Name (Fast "LIKE" queries)
CREATE INDEX IF NOT EXISTS idx_families_name_trgm ON public.families USING GIN (name gin_trgm_ops);

-- GIN Index for JSONB Parameters (Querying inside the JSON blob)
CREATE INDEX IF NOT EXISTS idx_families_parameters ON public.families USING GIN (parameters);

-- Standard B-Tree Indexes for Filtering
CREATE INDEX IF NOT EXISTS idx_families_category ON public.families (category);
CREATE INDEX IF NOT EXISTS idx_families_revit_version ON public.families (revit_version);
CREATE INDEX IF NOT EXISTS idx_families_status ON public.families (status);
CREATE INDEX IF NOT EXISTS idx_families_created_by ON public.families (created_by);

-- 5. STORAGE BUCKET CONFIGURATION (Optional - Run if using Storage)
-- ==============================================================================
-- Requires 'storage' schema access
-- insert into storage.buckets (id, name, public) values ('families', 'families', true);
-- insert into storage.buckets (id, name, public) values ('thumbnails', 'thumbnails', true);
