using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Mapping;

namespace Skywalker.Driver
{
	/// <summary>
	/// Interface for the navigator.
	/// </summary>
    public interface INavigator
    {
        
        /// <summary>
        /// Gets the direction angle.
        /// </summary>
        /// <param name="userInput">The user input angle.</param>
        /// <returns>The direction angle.</returns>
        Vector GetDirection(Vector userInput);

        /// <summary>
        /// Signals the navigator to find a route to the given name destination.
        /// </summary>
        /// <param name="destination">Name of location to route to.</param>
		void NavigateTo(mPoint destinationPointInUV);

        //sets up the starting vector used to navigate 
        //void setStartVector(Vector startVector);
 
        List<mPoint> GetRoute();
        List<mPoint> GetPath();

        //for testing dont use this
        void SetRoute(mPoint nextPointUV);

        void SetSpeed(double newSpeed);
        double GetSpeed();

        //Returns the current drive type for navigator. Used to color the output to the user
        // 0 for UserDriven
        // 1 for UnRestrictedUserDriven
        // 2 for RoutDriven
        // 3 for DoorDriven.
        // -1 for error.
        int GetCurrentDriveType();
        void setCurrentDriveType(int newStatus);

        void ToggleUsermode();
        void ToggleDoorNavigationMode();

        int getDistanceFromSonar(int id);

		int[] MotorCalibrationRun();

		int GetCalibrationResult();
    }
}
