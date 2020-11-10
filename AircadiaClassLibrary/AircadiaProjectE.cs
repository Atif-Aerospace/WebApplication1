//using System;
//using System.Collections.Generic;
//using System.Text;
//using Aircadia.ObjectModel;

//namespace Aircadia
//{
//	public class AircadiaProjectE : AircadiaProject
//	{

//		private static AircadiaProjectE instance;
//		public static AircadiaProjectE Current => instance = instance ?? new AircadiaProjectE();

//		public override bool AskUserOverwriteConfirmation(AircadiaComponent component, string type) => MessageBox.Show($"{type} \"{component.Name}\" already exists. Overwrite?", "Saving the data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

//		private override bool AskUserConfirmImportantChanges(AircadiaComponent component, string depString) => MessageBox.Show($"Important changes were made to \"{component.Name}\", which will invalidate dependent components:\r\n{depString}Do you want to delete this components? If you decided not to delete them the changes made to \"{component.Name}\" will be discarded", "Saving the data", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

//		private override bool AskUserDeleteDependencies(AircadiaComponent component, string depString) => MessageBox.Show($"The following components depend on {component.Name} and need to be deleted before it. Do you want to delete them?\r\n\r\nAffectedComponents:\r\n{depString}", "Data has dependencies", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
//	}
//}
