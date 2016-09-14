using UnityEngine;
using PickleTools.Extensions.ArrayExtensions;

namespace PickleTools.UnityEditor {

	public class SortableList<T> {

		T[] listEntries = new T[0];
		public T[] ListEntries {
			get { return listEntries; }
		}

		T dragEntry;
		public T DragEntry {
			get { return dragEntry; }
		}

		float entryHeight = 24.0f;
		public float EntryHeight {
			get { return entryHeight; }
			set { entryHeight = value; }
		}

		float topPadding = 2.0f;
		public float TopPadding {
			get { return topPadding; }
			set { topPadding = value; }
		}

		private Rect[] entryRects = new Rect[0];
		private float dragEntryListHeight = 0.0f;

		private bool dragMode = false;
		private int insertSlot = 0;
		private int removeIndex = 0;

		public SortableList(){
			listEntries = new T[0];
		}

		public SortableList(T[] newEntries, float newEntryHeight = 24.0f, float newTopPadding = 2.0f){
			listEntries = newEntries;
			entryHeight = newEntryHeight;
			topPadding = newTopPadding;
		}

#region draw
		// TODO: how do we display the entries within this draw function, so that
		// we can draw anything within the drag entries.
		// maybe we do a start and stop...?
		public T[] Draw(System.Action<T, float, int> callback, System.Action<T, Rect> dragCallback, T[] newListEntries = null){
			if(newListEntries != null){
				listEntries = newListEntries;
			}
			Rect lastRect = GUILayoutUtility.GetLastRect();
			dragEntryListHeight = 0;
			entryRects = new Rect[listEntries.Length];
			for(int e = 0; e < listEntries.Length; e ++){
				lastRect = new Rect(lastRect.x, lastRect.y + lastRect.height + topPadding, Screen.width, entryHeight);
				dragEntryListHeight += entryHeight + topPadding;
				// draw the entry
				callback(listEntries[e], entryHeight, e);
				// store the rect for input checking
				entryRects[e] = lastRect;
			}

			HandleInput(dragCallback);
			return listEntries;
		}
#endregion

#region input
		void HandleInput(System.Action<T, Rect> dragCallback){

			for(int e = 0; e < entryRects.Length; e ++){
				if(entryRects[e].Contains(Event.current.mousePosition) &&
				   Event.current.type == EventType.mouseDown && Event.current.button == 0){
					dragMode = true;
					dragEntry = listEntries[e];
					removeIndex = e;
					break;
				}
			}
			if(dragMode && Event.current.type == EventType.mouseUp){
				dragMode = false;
				// remove the entry from the list
				listEntries = listEntries.RemoveAt(removeIndex);
				// make sure the slot can be inserted within the range of entries
				insertSlot = Mathf.Clamp(insertSlot, 0, listEntries.Length);
				// insert the dragged entry
				listEntries = listEntries.InsertAt<T>(insertSlot, dragEntry);
				// reset values
				dragEntry = default(T);
				insertSlot = -1;
				removeIndex = -1;
			}

			if(dragMode && Event.current.type != EventType.layout){
				Rect dragBox = new Rect(0, Event.current.mousePosition.y - entryHeight * 0.5f,
				                        Screen.width, entryHeight);
				dragCallback(dragEntry, dragBox);
				// show where we are placing the entry
				for(int e = 0; e < entryRects.Length; e ++){
					// greater than 0 or entry-1 y + halfWidth
					float min = Mathf.Max(entryRects[e].y - entryHeight * 0.5f, 0);
					// less than entry y + half width
					float max = entryRects[e].y + entryHeight * 0.5f;
					
					if(Event.current.mousePosition.y >= min &&
					   Event.current.mousePosition.y <= max){
						GUI.color = Color.green;
						GUI.Box(new Rect(entryRects[e].x, entryRects[e].y + (topPadding * e - topPadding), Screen.width, 4), "");
						GUI.color = Color.white;
						insertSlot = e-1;
						break;
					} else if(e == entryRects.Length - 1 && Event.current.mousePosition.y > max){
						GUI.color = Color.green;
						GUI.Box(new Rect(entryRects[e].x, entryRects[e].y + entryHeight + (topPadding * e - topPadding), Screen.width, 4), "");
						GUI.color = Color.white;
						insertSlot = e;
					}
				}
			}
		}
#endregion

		public void DefaultDragDraw(T entryType, Rect dragRect){
			GUI.Box(dragRect, entryType.ToString());
		}

	}
}