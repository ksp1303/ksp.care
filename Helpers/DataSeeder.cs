using ksp.care.Models;

namespace ksp.care.Helpers
{
    public static class DataSeeder
    {
        public static void SeedAll(ApplicationDbContext db)
        {
            SeedDoctors(db);
            SeedTests(db);
            SyncHomepagePackages(db);
        }

        // Runs every startup: adds any sample doctor that isn't already present,
        // so databases created before this list existed still get the full panel.
        // Doctors added or edited by staff in Admin → Doctors are left untouched.
        private static void SeedDoctors(ApplicationDbContext db)
        {
            var doctors = new[]
            {
                new DoctorRecord { Name = "Arun Shetty",       Specialization = "General Physician",    Mobile = "9876543201" },
                new DoctorRecord { Name = "Priya Nayak",       Specialization = "Pathologist",          Mobile = "9876543202" },
                new DoctorRecord { Name = "Rajesh Kumar",      Specialization = "Microbiologist",       Mobile = "9876543203" },
                new DoctorRecord { Name = "Sneha Rao",         Specialization = "Biochemist",           Mobile = "9876543204" },
                new DoctorRecord { Name = "Vikram Hegde",      Specialization = "Haematologist",        Mobile = "9876543205" },
                new DoctorRecord { Name = "Deepa Kamath",      Specialization = "Endocrinologist",      Mobile = "9876543206" },
                new DoctorRecord { Name = "Suresh Pai",        Specialization = "Cardiologist",         Mobile = "9876543207" },
                new DoctorRecord { Name = "Kavitha Bhat",      Specialization = "Gynaecologist",        Mobile = "9876543208" }
            };

            foreach (var doc in doctors)
            {
                if (!db.Doctors.Any(d => d.Name == doc.Name || d.Mobile == doc.Mobile))
                    db.Doctors.Add(doc);
            }
            db.SaveChanges();
        }

        private static void SeedTests(ApplicationDbContext db)
        {
            if (db.Tests.Any()) return;

            var tests = new List<TestRecord>
            {
                // ── Haematology ──────────────────────────────────────────
                new() { TestName = "Complete Blood Count (CBC)",          Category = "Haematology",     Price = 350,   Description = "Includes WBC, RBC, Hb, PCV, Platelets, MCV, MCH, MCHC, RDW, differential count" },
                new() { TestName = "Erythrocyte Sedimentation Rate (ESR)", Category = "Haematology",    Price = 150,   Description = "Measures rate of RBC sedimentation — inflammation marker" },
                new() { TestName = "Haemoglobin (Hb)",                   Category = "Haematology",     Price = 100,   Description = "Blood haemoglobin concentration" },
                new() { TestName = "Peripheral Blood Smear (PBS)",       Category = "Haematology",     Price = 250,   Description = "Microscopic examination of blood cells morphology" },
                new() { TestName = "Reticulocyte Count",                 Category = "Haematology",     Price = 200,   Description = "Immature red blood cell count — bone marrow activity" },
                new() { TestName = "Platelet Count",                     Category = "Haematology",     Price = 150,   Description = "Thrombocyte count for bleeding disorders" },
                new() { TestName = "Total WBC Count",                    Category = "Haematology",     Price = 120,   Description = "White blood cell count — infection marker" },
                new() { TestName = "Blood Group & Rh Typing",           Category = "Haematology",     Price = 200,   Description = "ABO and Rh factor determination" },
                new() { TestName = "Coomb's Test (Direct)",              Category = "Haematology",     Price = 350,   Description = "Detects antibodies attached to red blood cells" },
                new() { TestName = "Coomb's Test (Indirect)",            Category = "Haematology",     Price = 400,   Description = "Detects antibodies in blood serum" },
                new() { TestName = "G6PD Quantitative",                  Category = "Haematology",     Price = 500,   Description = "Glucose-6-phosphate dehydrogenase enzyme level" },
                new() { TestName = "HbA1c (Glycosylated Haemoglobin)",  Category = "Haematology",     Price = 450,   Description = "3-month average blood sugar — diabetes monitoring" },
                new() { TestName = "Haemoglobin Electrophoresis",        Category = "Haematology",     Price = 800,   Description = "Identifies abnormal haemoglobin variants (Thalassemia, Sickle Cell)" },

                // ── Coagulation ──────────────────────────────────────────
                new() { TestName = "Prothrombin Time (PT/INR)",          Category = "Coagulation",     Price = 350,   Description = "Extrinsic clotting pathway — warfarin monitoring" },
                new() { TestName = "Activated Partial Thromboplastin Time (APTT)", Category = "Coagulation", Price = 400, Description = "Intrinsic clotting pathway assessment" },
                new() { TestName = "Bleeding Time (BT)",                 Category = "Coagulation",     Price = 100,   Description = "Time taken for bleeding to stop from a standardized wound" },
                new() { TestName = "Clotting Time (CT)",                 Category = "Coagulation",     Price = 100,   Description = "Time taken for blood to clot in vitro" },
                new() { TestName = "D-Dimer",                            Category = "Coagulation",     Price = 900,   Description = "Fibrin degradation product — DVT/PE screening" },
                new() { TestName = "Fibrinogen Level",                   Category = "Coagulation",     Price = 600,   Description = "Clotting factor I level" },

                // ── Biochemistry ─────────────────────────────────────────
                new() { TestName = "Fasting Blood Sugar (FBS)",          Category = "Biochemistry",    Price = 100,   Description = "Glucose level after 8-12 hours fasting" },
                new() { TestName = "Post Prandial Blood Sugar (PPBS)",   Category = "Biochemistry",    Price = 100,   Description = "Glucose level 2 hours after meal" },
                new() { TestName = "Random Blood Sugar (RBS)",           Category = "Biochemistry",    Price = 100,   Description = "Glucose level at any time of day" },
                new() { TestName = "Oral Glucose Tolerance Test (OGTT)", Category = "Biochemistry",    Price = 300,   Description = "Fasting + 1hr + 2hr glucose after 75g glucose load" },
                new() { TestName = "Serum Uric Acid",                   Category = "Biochemistry",    Price = 200,   Description = "Uric acid level — gout and kidney stone risk" },
                new() { TestName = "Serum Calcium",                     Category = "Biochemistry",    Price = 200,   Description = "Total calcium level in blood" },
                new() { TestName = "Serum Phosphorus",                  Category = "Biochemistry",    Price = 200,   Description = "Inorganic phosphate level" },
                new() { TestName = "Serum Magnesium",                   Category = "Biochemistry",    Price = 300,   Description = "Magnesium level — muscle and nerve function" },
                new() { TestName = "Serum Sodium",                      Category = "Biochemistry",    Price = 200,   Description = "Electrolyte balance — sodium level" },
                new() { TestName = "Serum Potassium",                   Category = "Biochemistry",    Price = 200,   Description = "Electrolyte balance — potassium level" },
                new() { TestName = "Serum Chloride",                    Category = "Biochemistry",    Price = 200,   Description = "Electrolyte balance — chloride level" },
                new() { TestName = "Serum Iron",                        Category = "Biochemistry",    Price = 250,   Description = "Iron level in blood" },
                new() { TestName = "Total Iron Binding Capacity (TIBC)", Category = "Biochemistry",   Price = 300,   Description = "Blood's capacity to bind iron — anaemia workup" },
                new() { TestName = "Serum Ferritin",                    Category = "Biochemistry",    Price = 500,   Description = "Iron storage protein — iron deficiency and overload" },
                new() { TestName = "Iron Profile (Iron + TIBC + Ferritin)", Category = "Biochemistry", Price = 900,  Description = "Complete iron status assessment" },
                new() { TestName = "Serum Amylase",                     Category = "Biochemistry",    Price = 350,   Description = "Pancreatic enzyme — pancreatitis marker" },
                new() { TestName = "Serum Lipase",                      Category = "Biochemistry",    Price = 400,   Description = "Pancreatic enzyme — more specific for pancreatitis" },
                new() { TestName = "LDH (Lactate Dehydrogenase)",       Category = "Biochemistry",    Price = 300,   Description = "Tissue damage marker — found in many organs" },
                new() { TestName = "CPK (Creatine Phosphokinase)",      Category = "Biochemistry",    Price = 350,   Description = "Muscle damage marker — heart and skeletal muscle" },

                // ── Liver Function Tests (LFT) ──────────────────────────
                new() { TestName = "Liver Function Test (LFT) - Complete", Category = "Liver Function", Price = 600, Description = "Bilirubin (Total/Direct), SGOT, SGPT, ALP, GGT, Total Protein, Albumin, Globulin, A/G Ratio" },
                new() { TestName = "SGOT (AST)",                        Category = "Liver Function",  Price = 200,   Description = "Aspartate aminotransferase — liver/heart enzyme" },
                new() { TestName = "SGPT (ALT)",                        Category = "Liver Function",  Price = 200,   Description = "Alanine aminotransferase — liver-specific enzyme" },
                new() { TestName = "Alkaline Phosphatase (ALP)",        Category = "Liver Function",  Price = 200,   Description = "Bone and liver enzyme" },
                new() { TestName = "GGT (Gamma GT)",                    Category = "Liver Function",  Price = 300,   Description = "Gamma-glutamyl transferase — liver and bile duct marker" },
                new() { TestName = "Serum Bilirubin (Total & Direct)",  Category = "Liver Function",  Price = 250,   Description = "Bilirubin levels — jaundice assessment" },
                new() { TestName = "Serum Albumin",                     Category = "Liver Function",  Price = 200,   Description = "Major blood protein produced by liver" },
                new() { TestName = "Total Protein",                     Category = "Liver Function",  Price = 200,   Description = "Total serum protein — albumin + globulin" },

                // ── Kidney Function Tests (KFT/RFT) ─────────────────────
                new() { TestName = "Kidney Function Test (KFT) - Complete", Category = "Kidney Function", Price = 600, Description = "Urea, Creatinine, BUN, Uric Acid, Electrolytes, eGFR" },
                new() { TestName = "Blood Urea",                        Category = "Kidney Function", Price = 150,   Description = "Urea level — kidney filtration marker" },
                new() { TestName = "Serum Creatinine",                  Category = "Kidney Function", Price = 200,   Description = "Creatinine level — kidney function assessment" },
                new() { TestName = "Blood Urea Nitrogen (BUN)",         Category = "Kidney Function", Price = 200,   Description = "Nitrogen portion of urea — kidney marker" },
                new() { TestName = "BUN/Creatinine Ratio",              Category = "Kidney Function", Price = 300,   Description = "Ratio to differentiate pre-renal vs renal causes" },
                new() { TestName = "eGFR (Estimated GFR)",              Category = "Kidney Function", Price = 250,   Description = "Estimated glomerular filtration rate — kidney staging" },
                new() { TestName = "Microalbumin (Urine)",              Category = "Kidney Function", Price = 500,   Description = "Early kidney damage detection in diabetes" },

                // ── Lipid Profile ────────────────────────────────────────
                new() { TestName = "Lipid Profile - Complete",          Category = "Lipid Profile",   Price = 500,   Description = "Total Cholesterol, HDL, LDL, VLDL, Triglycerides, TC/HDL Ratio" },
                new() { TestName = "Total Cholesterol",                 Category = "Lipid Profile",   Price = 200,   Description = "Total blood cholesterol level" },
                new() { TestName = "HDL Cholesterol",                   Category = "Lipid Profile",   Price = 200,   Description = "High-density lipoprotein — 'good' cholesterol" },
                new() { TestName = "LDL Cholesterol",                   Category = "Lipid Profile",   Price = 200,   Description = "Low-density lipoprotein — 'bad' cholesterol" },
                new() { TestName = "Triglycerides",                     Category = "Lipid Profile",   Price = 200,   Description = "Blood fat level — cardiovascular risk factor" },
                new() { TestName = "VLDL Cholesterol",                  Category = "Lipid Profile",   Price = 200,   Description = "Very low-density lipoprotein cholesterol" },
                new() { TestName = "Apolipoprotein A1",                 Category = "Lipid Profile",   Price = 700,   Description = "Major protein of HDL — cardiovascular risk" },
                new() { TestName = "Apolipoprotein B",                  Category = "Lipid Profile",   Price = 700,   Description = "Major protein of LDL — atherogenic risk" },

                // ── Thyroid Profile ──────────────────────────────────────
                new() { TestName = "Thyroid Profile (T3, T4, TSH)",     Category = "Thyroid",         Price = 500,   Description = "Complete thyroid function — T3, T4, and TSH levels" },
                new() { TestName = "TSH (Thyroid Stimulating Hormone)", Category = "Thyroid",         Price = 250,   Description = "Primary thyroid screening test" },
                new() { TestName = "Free T3 (FT3)",                     Category = "Thyroid",         Price = 300,   Description = "Free triiodothyronine — active thyroid hormone" },
                new() { TestName = "Free T4 (FT4)",                     Category = "Thyroid",         Price = 300,   Description = "Free thyroxine — thyroid hormone" },
                new() { TestName = "Anti-TPO Antibodies",               Category = "Thyroid",         Price = 700,   Description = "Thyroid peroxidase antibodies — autoimmune thyroid disease" },
                new() { TestName = "Anti-Thyroglobulin Antibodies",     Category = "Thyroid",         Price = 800,   Description = "Thyroglobulin antibodies — Hashimoto's diagnosis" },

                // ── Vitamins & Minerals ──────────────────────────────────
                new() { TestName = "Vitamin D (25-OH)",                 Category = "Vitamins",        Price = 800,   Description = "25-hydroxyvitamin D — bone health and immunity" },
                new() { TestName = "Vitamin B12",                       Category = "Vitamins",        Price = 700,   Description = "Cobalamin level — nerve function and anaemia" },
                new() { TestName = "Folate (Vitamin B9)",               Category = "Vitamins",        Price = 600,   Description = "Folic acid level — cell growth and pregnancy" },
                new() { TestName = "Vitamin A",                         Category = "Vitamins",        Price = 1200,  Description = "Retinol level — vision and immune function" },
                new() { TestName = "Vitamin E",                         Category = "Vitamins",        Price = 1500,  Description = "Tocopherol level — antioxidant" },
                new() { TestName = "Vitamin B1 (Thiamine)",             Category = "Vitamins",        Price = 1200,  Description = "Thiamine level — nerve and energy metabolism" },
                new() { TestName = "Vitamin B6 (Pyridoxine)",           Category = "Vitamins",        Price = 1200,  Description = "Pyridoxine level — brain development and function" },
                new() { TestName = "Zinc",                              Category = "Vitamins",        Price = 400,   Description = "Zinc level — immunity and wound healing" },

                // ── Hormones ─────────────────────────────────────────────
                new() { TestName = "Testosterone (Total)",              Category = "Hormones",        Price = 500,   Description = "Total testosterone level" },
                new() { TestName = "Testosterone (Free)",               Category = "Hormones",        Price = 900,   Description = "Bioavailable testosterone" },
                new() { TestName = "Prolactin",                         Category = "Hormones",        Price = 500,   Description = "Pituitary hormone — reproductive health" },
                new() { TestName = "FSH (Follicle Stimulating Hormone)", Category = "Hormones",       Price = 450,   Description = "Reproductive hormone — fertility assessment" },
                new() { TestName = "LH (Luteinizing Hormone)",         Category = "Hormones",        Price = 450,   Description = "Reproductive hormone — ovulation and fertility" },
                new() { TestName = "Estradiol (E2)",                    Category = "Hormones",        Price = 550,   Description = "Primary female sex hormone" },
                new() { TestName = "Progesterone",                      Category = "Hormones",        Price = 500,   Description = "Hormone essential for pregnancy" },
                new() { TestName = "DHEA-S",                            Category = "Hormones",        Price = 700,   Description = "Dehydroepiandrosterone sulfate — adrenal function" },
                new() { TestName = "Cortisol (Morning)",                Category = "Hormones",        Price = 500,   Description = "Stress hormone — adrenal function" },
                new() { TestName = "Insulin (Fasting)",                 Category = "Hormones",        Price = 500,   Description = "Fasting insulin level — insulin resistance screening" },
                new() { TestName = "Beta-hCG (Pregnancy Test)",         Category = "Hormones",        Price = 400,   Description = "Quantitative pregnancy hormone — confirms and monitors pregnancy" },
                new() { TestName = "PTH (Parathyroid Hormone)",         Category = "Hormones",        Price = 800,   Description = "Calcium regulation hormone" },
                new() { TestName = "Growth Hormone (GH)",               Category = "Hormones",        Price = 700,   Description = "Pituitary growth hormone level" },

                // ── Cardiac Markers ──────────────────────────────────────
                new() { TestName = "Troponin I (High Sensitivity)",     Category = "Cardiac Markers", Price = 800,   Description = "Heart muscle damage marker — acute MI diagnosis" },
                new() { TestName = "Troponin T",                        Category = "Cardiac Markers", Price = 700,   Description = "Cardiac-specific troponin — heart attack diagnosis" },
                new() { TestName = "CK-MB (Creatine Kinase-MB)",        Category = "Cardiac Markers", Price = 400,   Description = "Heart-specific muscle enzyme" },
                new() { TestName = "hs-CRP (High Sensitivity CRP)",     Category = "Cardiac Markers", Price = 500,   Description = "Inflammation marker — cardiovascular risk assessment" },
                new() { TestName = "BNP (Brain Natriuretic Peptide)",    Category = "Cardiac Markers", Price = 1200,  Description = "Heart failure marker" },
                new() { TestName = "Homocysteine",                      Category = "Cardiac Markers", Price = 800,   Description = "Amino acid — cardiovascular and stroke risk" },
                new() { TestName = "Lipoprotein(a)",                    Category = "Cardiac Markers", Price = 900,   Description = "Genetic cardiovascular risk factor" },

                // ── Urine Analysis ───────────────────────────────────────
                new() { TestName = "Urine Routine & Microscopy",       Category = "Urine Analysis",  Price = 150,   Description = "Physical, chemical, and microscopic urine examination" },
                new() { TestName = "Urine Culture & Sensitivity",      Category = "Urine Analysis",  Price = 600,   Description = "Bacterial culture with antibiotic sensitivity — UTI diagnosis" },
                new() { TestName = "24-Hour Urine Protein",            Category = "Urine Analysis",  Price = 300,   Description = "Total protein in 24-hour urine collection — kidney assessment" },
                new() { TestName = "Urine Microalbumin/Creatinine Ratio", Category = "Urine Analysis", Price = 600, Description = "Spot urine — early diabetic nephropathy screening" },
                new() { TestName = "Urine Pregnancy Test (UPT)",       Category = "Urine Analysis",  Price = 150,   Description = "Qualitative hCG detection in urine" },

                // ── Serology & Immunology ─────────────────────────────────
                new() { TestName = "CRP (C-Reactive Protein)",         Category = "Serology",        Price = 350,   Description = "General inflammation marker" },
                new() { TestName = "ASO Titre (Anti-Streptolysin O)",  Category = "Serology",        Price = 350,   Description = "Streptococcal infection marker — rheumatic fever" },
                new() { TestName = "RA Factor (Rheumatoid Factor)",    Category = "Serology",        Price = 400,   Description = "Autoimmune marker — rheumatoid arthritis screening" },
                new() { TestName = "ANA (Anti-Nuclear Antibodies)",    Category = "Serology",        Price = 800,   Description = "Autoimmune screening — SLE, lupus" },
                new() { TestName = "Widal Test",                       Category = "Serology",        Price = 250,   Description = "Typhoid fever antibody detection" },
                new() { TestName = "Dengue NS1 Antigen",               Category = "Serology",        Price = 600,   Description = "Early dengue fever detection" },
                new() { TestName = "Dengue IgM/IgG",                   Category = "Serology",        Price = 700,   Description = "Dengue antibody — current or past infection" },
                new() { TestName = "Malaria Antigen (Rapid)",          Category = "Serology",        Price = 400,   Description = "Rapid malaria parasite detection" },
                new() { TestName = "HIV 1 & 2 Antibody",               Category = "Serology",        Price = 400,   Description = "HIV screening test" },
                new() { TestName = "HBsAg (Hepatitis B Surface Antigen)", Category = "Serology",     Price = 350,   Description = "Hepatitis B screening" },
                new() { TestName = "Anti-HCV (Hepatitis C Antibody)",  Category = "Serology",        Price = 500,   Description = "Hepatitis C screening" },
                new() { TestName = "VDRL (Syphilis Screening)",        Category = "Serology",        Price = 250,   Description = "Syphilis screening test" },
                new() { TestName = "PSA (Prostate Specific Antigen)",  Category = "Serology",        Price = 600,   Description = "Prostate cancer screening marker" },
                new() { TestName = "CA-125 (Ovarian Cancer Marker)",   Category = "Serology",        Price = 900,   Description = "Tumour marker — ovarian cancer screening" },
                new() { TestName = "CEA (Carcinoembryonic Antigen)",   Category = "Serology",        Price = 800,   Description = "Tumour marker — colorectal and other cancers" },
                new() { TestName = "AFP (Alpha-Fetoprotein)",          Category = "Serology",        Price = 700,   Description = "Tumour marker — liver cancer and pregnancy screening" },

                // ── Health Packages (extras not advertised on homepage) ──
                new() { TestName = "Senior Citizen Package",           Category = "Health Package",   Price = 3999,  Description = "CBC, LFT, KFT, Lipid Profile, Thyroid, HbA1c, Vitamin D, B12, PSA/CA-125, ECG, Urine" },
                new() { TestName = "Fever Panel",                      Category = "Health Package",   Price = 1799,  Description = "CBC, ESR, CRP, Widal, Dengue NS1, Dengue IgM/IgG, Malaria Antigen, Urine Routine, LFT" },
                new() { TestName = "Anaemia Profile",                  Category = "Health Package",   Price = 1299,  Description = "CBC, Iron Profile, Vitamin B12, Folate, Reticulocyte Count, Peripheral Smear" }
            };

            db.Tests.AddRange(tests);
            db.SaveChanges();
        }

        // The 8 packages advertised on the homepage (Views/Home/Index.cshtml).
        // Runs every startup: renames legacy seeder names, adds missing entries.
        // Prices are only set on create so admin edits in Admin → Tests survive.
        private static void SyncHomepagePackages(ApplicationDbContext db)
        {
            var renames = new Dictionary<string, string>
            {
                ["Basic Master Health Checkup"] = "Basic Master Health",
                ["Comprehensive Body Checkup"]  = "Comprehensive Body",
                ["Cardiac Health Package"]      = "Cardiac Care",
                ["Diabetes Screening Package"]  = "Diabetes Screening",
                ["Thyroid Care Package"]        = "Thyroid Panel",
                ["Women's Wellness Package"]    = "Women's Health",
                ["Vitamin Profile Package"]     = "Vitamin Profile",
            };

            foreach (var (oldName, newName) in renames)
            {
                var rec = db.Tests.FirstOrDefault(t => t.TestName == oldName);
                if (rec != null && !db.Tests.Any(t => t.TestName == newName))
                    rec.TestName = newName;
            }

            var packages = new (string Name, decimal Price, string Desc)[]
            {
                ("Basic Master Health",  1499m, "CBC, FBS, Lipid Profile, LFT, KFT, Urine Routine"),
                ("Comprehensive Body",   2999m, "All Basic tests + Thyroid Profile, HbA1c, Iron Profile, Vitamin D, Vitamin B12"),
                ("Vitamin Profile",       999m, "Vitamin D, B12, Calcium, Folate"),
                ("Cardiac Care",         1799m, "Lipid Profile, hs-CRP, Homocysteine, FBS, HbA1c"),
                ("Diabetes Screening",   1299m, "FBS, PPBS, HbA1c, Fasting Insulin, HOMA-IR, KFT, Urine Microalbumin, Lipid Profile"),
                ("Thyroid Panel",         899m, "T3, T4, TSH, Free T3, Free T4, Anti-TPO, Anti-Thyroglobulin"),
                ("Liver & Kidney Panel", 1599m, "LFT, KFT, Electrolytes, GGT, Urine Routine, eGFR"),
                ("Women's Health",       2499m, "CBC, Iron & Ferritin, Thyroid, Vitamin D, B12, Calcium, FSH, LH, Prolactin, Lipid Profile, FBS"),
            };

            foreach (var (name, price, desc) in packages)
            {
                var rec = db.Tests.FirstOrDefault(t => t.TestName == name);
                if (rec == null)
                {
                    db.Tests.Add(new TestRecord { TestName = name, Category = "Health Package", Price = price, Description = desc });
                }
                else
                {
                    rec.Category = "Health Package";
                    rec.IsActive = true;
                }
            }
            db.SaveChanges();
        }
    }
}
