// Source: https://gist.github.com/benblo/10732554
using UnityEditor;
using System.Collections;

namespace PickleTools.UnityEditor {
	public class EditorCoroutine {
		public static EditorCoroutine start(IEnumerator _routine) {
			EditorCoroutine coroutine = new EditorCoroutine(_routine);
			coroutine.start();
			return coroutine;
		}

		readonly IEnumerator routine;
		EditorCoroutine(IEnumerator _routine) {
			routine = _routine;
		}

		void start() {
			UnityEngine.Debug.Log("start");
			EditorApplication.update += update;
		}
		public void stop() {
			UnityEngine.Debug.Log("stop");
			EditorApplication.update -= update;
		}

		void update() {
			/* NOTE: no need to try/catch MoveNext,
				 * if an IEnumerator throws its next iteration returns false.
				 * Also, Unity probably catches when calling EditorApplication.update.
				 */

			UnityEngine.Debug.Log("update");
			if (!routine.MoveNext()) {
				stop();
			}
		}
	}
}