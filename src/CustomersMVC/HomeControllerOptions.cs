// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

namespace CustomersMVC
{
    /// <summary>
    /// Configuration: Here we are creating a class that will used to set the Title in the index page for the HomeController.
    ///                These types of options should follow the Interface Segragation Principle (ISP) and Seperation of Concerns
    ///                (ex: seperate configuration options classes for different logical parts of the application).
    /// </summary>
    public class HomeControllerOptions
    {
        public string Title { get; set; }
    }
}
