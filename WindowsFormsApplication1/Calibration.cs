/*
 * They made me program in C#
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace BeachScouter
{
    public class Calibration
    {
        List<MCvPoint3D32f[]> worldPoints_list = new List<MCvPoint3D32f[]>();
        MCvPoint3D32f[][] worldPoints = new MCvPoint3D32f[1][]; //jagged array (array of arrays)
        List<PointF[]> imagePoints_list = new List<PointF[]>();
        PointF[][] imagePoints = new PointF[1][];
        IntrinsicCameraParameters intrinsic;
        ExtrinsicCameraParameters[] extrinsic;
        Size frame_size; 


        public Calibration(Size framesize) {
            frame_size = framesize;
            List<MCvPoint3D32f[]> worldPoints_list = new List<MCvPoint3D32f[]>();
            List<PointF[]> imagePoints_list = new List<PointF[]>();
        }


        public void addMapping(float image_x, float image_y, float world_x, float world_y, float world_z)
        {
            imagePoints_list.Add(new PointF[] { new PointF(image_x, image_y) });
            worldPoints_list.Add(new MCvPoint3D32f[] { new MCvPoint3D32f(world_x, world_y, 0) }); // for now just assume that z=0 (BirdView)

        }

        public void clearMappings()
        {
            imagePoints_list.Clear();
            worldPoints_list.Clear();
        }


        public PointF Calibrate(float image_x, float image_y)
        {
            // If not enough mapping points are giving, don't do the calibration
            if (worldPoints_list.Count < 4)
                return new PointF(0, 0);

            
            try
            {
                intrinsic = new IntrinsicCameraParameters();
            }
            catch (Exception e) { MessageBox.Show(e.InnerException.Message); }

            worldPoints[0] = new MCvPoint3D32f[] { (worldPoints_list[0])[0], (worldPoints_list[1])[0], (worldPoints_list[2])[0], (worldPoints_list[3])[0] };
            imagePoints[0] = new PointF[] { (imagePoints_list[0])[0], (imagePoints_list[1])[0], (imagePoints_list[2])[0], (imagePoints_list[3])[0] };
   
            
            CameraCalibration.CalibrateCamera(worldPoints, imagePoints, frame_size, intrinsic, CALIB_TYPE.DEFAULT, out extrinsic);


            Matrix<double> extrinsicMatrix = extrinsic[0].ExtrinsicMatrix;

            // Build the H matrix out of the extrinsic pramater Matrix.
            // we dont use the 3rd colunm, since we assume that world_z=0 (multiplying by world_z=0 would result to 0 anyways)
            Matrix<double> H = new Matrix<double>(3, 3);
            H[0, 0] = extrinsicMatrix[0, 0]; H[0, 1] = extrinsicMatrix[0, 1]; H[0, 2] = extrinsicMatrix[0, 3];
            H[1, 0] = extrinsicMatrix[1, 0]; H[1, 1] = extrinsicMatrix[1, 1]; H[1, 2] = extrinsicMatrix[1, 3];
            H[2, 0] = extrinsicMatrix[2, 0]; H[2, 1] = extrinsicMatrix[2, 1]; H[2, 2] = extrinsicMatrix[2, 3];
            Matrix<double> H_inverse = new Matrix<double>(3, 3);
            CvInvoke.cvInvert(H, H_inverse, SOLVE_METHOD.CV_LU); 


            // intrinsic parameters include focal length, offset, etc
            Matrix<double> intrinsicMatrix_inverse = new Matrix<double>(3, 3);
            CvInvoke.cvInvert(intrinsic.IntrinsicMatrix, intrinsicMatrix_inverse, SOLVE_METHOD.CV_LU);

            Matrix<double> HI = new Matrix<double>(3, 3);
            HI = H_inverse.Mul(intrinsicMatrix_inverse);


            // This is the image_point we want to transform to world 3D coordinates
            // we use the Homogeneous coordinate system, which allows us to use Matrix multiplications
            // needed for translation. We also use z=1 because we assume that the camera image plane
            // is at z=1. We will fix this later with the intrinsic parameter matrix (inverse) multiplication
            Matrix<double> arbitraryPointMatrix = new Matrix<double>(3, 1);
            arbitraryPointMatrix[0, 0] = image_x;
            arbitraryPointMatrix[1, 0] = image_y;
            arbitraryPointMatrix[2, 0] = 1;


            // Do the projective transformation
            Matrix<double> result3DMatrix = new Matrix<double>(3, 1);
            result3DMatrix = (HI).Mul(arbitraryPointMatrix);


            // Get the point in Homogeneous coordinate system with z=1 by dividing with z = result3DMatrix[2, 0]
            // (z=1, because the image plane is at z=1)
            PointF point3D = new PointF();
            point3D.X = (float)(result3DMatrix[0, 0] / result3DMatrix[2, 0]);
            point3D.Y = (float)(result3DMatrix[1, 0] / result3DMatrix[2, 0]);

            return point3D;
            
        }
    }
}
