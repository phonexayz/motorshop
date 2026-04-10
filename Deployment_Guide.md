# Cloud Deployment Guide (MotorShop)

This guide will help you move your system to the Cloud so you can turn off your computer and still access it from anywhere.

## Step 1: Create a Cloud Database (Supabase)
1. Go to [supabase.com](https://supabase.com) and Sign Up with your GitHub account.
2. Create a **New Project**.
3. Set a **Database Password** (Remember this!).
4. Once created, go to **Settings > Database** and copy your **Connection String** (URI format).

## Step 2: Export your current data
Run this command in your PC terminal to save your current data to a file:
```powershell
pg_dump -U postgres -d motorshop_db --clean --if-exists --no-owner --no-privileges > motorshop_backup.sql
```
*(Note: Replace `motorshop_db` with your actual local database name if different)*

## Step 3: Import data to Supabase
1. In your Supabase dashboard, go to **SQL Editor**.
2. Open the `motorshop_backup.sql` file on your PC, copy all text.
3. Paste it into the Supabase SQL Editor and click **Run**.
4. Now your Cloud database has all your products and customers!

## Step 4: Push code to GitHub
1. Create a **New Repository** on GitHub (name it `motorshop`).
2. Run these commands in your project folder:
```powershell
git init
git add .
git commit -m "Initial cloud ready commit"
git remote add origin https://github.com/phonexayz/motorshop.git
git push -u origin main
```

## Step 5: Run on the Website (Railway.app)
1. Go to [railway.app](https://railway.app) and login with GitHub.
2. Click **New Project** > **Deploy from GitHub repo**.
3. Select your `motorshop` repo.
4. Go to **Variables** and add these:
   - `DATABASE_URL`: (Paste your Supabase URI string here)
   - `ASPNETCORE_ENVIRONMENT`: `Production`
5. Railway will automatically build and give you a URL (e.g., `motorshop-production.up.railway.app`).

**DONE!** You can now access your shop from your phone or any computer.
