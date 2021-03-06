﻿using Android.App;
using Android.Widget;
using Android.OS;
using DSoft.UI.Grid;
using System;
using Truii_Demo_App.Data.Grid;
using AChartEngine.Models;
using AChartEngine.Renderers;
using Android.Graphics;
using Android.Content;
using AChartEngine;
using AChartEngine.Charts;
using Android.Content.Res;
using Java.IO;
using System.Collections.Generic;

namespace Truii_Demo_App
{
    [Activity(Label = "Truii", MainLauncher = false, Icon = "@drawable/icon")]
    public class HomeActivity : Activity
    {
        DSGridView dsGrid;
        Button btnCollect;
        Button btnGraph;
        Button btnExport;
        Button btnReset;
        DataDB db;

        /// <summary>
        /// This Code Assigns all the buttons to the respective code and assigns the grid
        /// to the initialized spreadsheet
        /// </summary>
        /// <param name="bundle">Used for Generating the page</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Home);
            db = new DataDB(this);

            dsGrid = FindViewById<DSGridView>(Resource.Id.dataGrid);
            if (dsGrid != null)
            {
                dsGrid.DataSource = new DataSet(this);
                dsGrid.TableName = "DT";
            }
            dsGrid.SetMinimumHeight(Resources.DisplayMetrics.HeightPixels / 2);
            btnCollect = FindViewById<Button>(Resource.Id.btnCollect);
            btnCollect.Click += BtnCollect_Click;

            btnGraph = FindViewById<Button>(Resource.Id.btnGraph);
            btnGraph.Click += BtnGraph_Click;

            btnExport = FindViewById<Button>(Resource.Id.btnExport);
            btnExport.Click += BtnExport_Click;

            btnReset = FindViewById<Button>(Resource.Id.btnReset);
            btnReset.Click += BtnReset_Click;
        }

        /// <summary>
        /// This Function Navigates the User to the Collecting Data Page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCollect_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(CollectActivity)));
        }

        /// <summary>
        /// This Function Calls the Graph Function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGraph_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(ChartActivity)));
        }

        /// <summary>
        /// This Function Exports a csv file to the device's local storage 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_Click(object sender, EventArgs e)
        {
            //Gets the file path
            File store = Android.OS.Environment.ExternalStorageDirectory;
            File dir = new File(store.AbsoluteFile.ToString());
            //Creates a new file
            File file = new File(dir, "DataTableExample.csv");
            file.CreateNewFile();
            file.Mkdirs();
            //Writes the .csv code into the .csv file
            FileWriter writer = new FileWriter(file);
            //File is being written
            writer.Write(csvCode());
            writer.Flush();
            writer.Close();
            //Alerts the user the file has been gerneated
            var alert = new AlertDialog.Builder(this);
            alert.SetTitle("A file has been created");
            alert.SetMessage("File is located at: " + dir.ToString() + "/DataTableExample.csv");
            alert.Show();
        }

        /// <summary>
        /// The csv export code
        /// </summary>
        /// <returns>The csv code of the data fromt the database</returns>
        private string csvCode()
        {
            string file = "UserID, DataOne, DataTwo, DataThree\n";
            for (int i = 0; i < db.Count(); i += 1)
            {
                file += db.readPrimary("UserID", i) + ", ";
                file += db.readData("DataOne", i) + ", ";
                file += db.readData("DataTwo", i) + ", ";
                file += db.readData("DataThree", i) + ",\n";
            }
            return file;
        }

        /// <summary>
        /// This Function wipes all information from the database to 
        /// reset it then updates the page with the OnResume();
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReset_Click(object sender, EventArgs e)
        {
            db.CreateDatabase();
            OnResume();
        }

        /// <summary>
        /// The Function used to constantly update the page for whatever changes
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            if (dsGrid != null)
            {
                dsGrid.DataSource = new DataSet(this);
                dsGrid.TableName = "DT";
            }

            if (db.Count() < 2)
            {
                btnGraph.Enabled = false;
            }
            else
            {
                btnGraph.Enabled = true;
            }
        }
        /*
        /// <summary>
        /// This Function uses data from the database to consrtuct a graph
        /// Using the three: DataOne, DataTwo and DataThree as the series
        /// Renders all the code into into a graph for data visualization
        /// </summary>
        private void LineGraph()
        {
            XYMultipleSeriesDataset dataSet = new XYMultipleSeriesDataset();
            
            dataSet.AddSeries(SeriesCreate("DataOne"));
            dataSet.AddSeries(SeriesCreate("DataTwo"));
            dataSet.AddSeries(SeriesCreate("DataThree"));

            XYSeriesRenderer renderOne = singleRenderer(255, 255, 000);
            XYSeriesRenderer renderTwo = singleRenderer(000, 255, 000);
            XYSeriesRenderer renderThree = singleRenderer(000, 255, 255);

            XYMultipleSeriesRenderer mRenderer = new XYMultipleSeriesRenderer();
            mRenderer.SetMargins(new int[] { 20, 30, 20, 10 });
            mRenderer.XLabels = 0;
            mRenderer.ChartTitle = "Data Chart";
            mRenderer.XTitle = "UserID";
            mRenderer.YTitle = "Data Inputs";
            mRenderer.AxisTitleTextSize = 32;
            mRenderer.ChartTitleTextSize = 40;
            mRenderer.LabelsTextSize = 20;
            mRenderer.LegendTextSize = 20;
            mRenderer.PointSize = 3;
            mRenderer.ZoomButtonsVisible = true;
            
            mRenderer.BackgroundColor = Color.Transparent;
            mRenderer.AxesColor = Color.DarkGray;
            mRenderer.LabelsColor = Color.LightGray;
            for (int i = 0; i < db.Count(); i++)
            {
                mRenderer.AddXTextLabel(i, db.readPrimary("UserID", i));
            }
            mRenderer.AddSeriesRenderer(renderOne);
            mRenderer.AddSeriesRenderer(renderTwo);
            mRenderer.AddSeriesRenderer(renderThree);

            Intent intent = ChartFactory.GetLineChartIntent(this, dataSet, mRenderer);
            StartActivity(intent);
        }

        /// <summary>
        /// This code is an efficient way of mass producing series for the graph
        /// Using the information from a certain column and collects all the data
        /// to input into the series
        /// </summary>
        /// <param name="name">Name of the Column of the Data as well as the Name of the newly created series</param>
        /// <returns>Returns the new series which contains information from the database</returns>
        public XYSeries SeriesCreate(string name)
        {
            XYSeries series = new XYSeries(name);
            for (int i = 0; i < db.Count(); i++)
            {
                series.Add(i, db.readData(name, i));
            }
            return series;
        }

        /// <summary>
        /// This code creates a renderer for a series
        /// </summary>
        /// <param name="Red">A Colour Code used to define the Series Renderer visually</param>
        /// <param name="Green">A Colour Code used to define the Series Renderer visually</param>
        /// <param name="Blue">A Colour Code used to define the Series Renderer visually</param>
        /// <returns>Returns the Renderer after setting all the desired traits</returns>
        public XYSeriesRenderer singleRenderer(int Red, int Green, int Blue)
        {
            XYSeriesRenderer sRender = new XYSeriesRenderer();
            sRender.FillPoints = true;
            sRender.LineWidth = 1;
            sRender.DisplayChartValues = true;
            sRender.ChartValuesTextSize = 25;
            sRender.PointStyle = PointStyle.Circle;
            sRender.Color = Color.Rgb(Red, Green, Blue);
            return sRender;
        }*/
    }
}

