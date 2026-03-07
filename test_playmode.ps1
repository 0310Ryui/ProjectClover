$ErrorActionPreference = "Stop"
chcp 65001

Write-Host "1. Setting Timer duration to 5s for fast testing..."
uloop execute-dynamic-code --code 'var tm = GameObject.Find("MainManager").GetComponent<TimerManager>(); tm.workDurationSeconds = 5f; UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()); return "Done";'

Write-Host "2. Entering Play Mode..."
uloop control-play-mode --action play
Start-Sleep -Seconds 5

Write-Host "3. Getting Initial Money..."
uloop execute-dynamic-code --code 'return SaveManager.Current.player.inventory.money.ToString();'

Write-Host "4. Clicking Work Button..."
uloop execute-dynamic-code --code 'var btn = GameObject.Find("Btn_Timer").GetComponent<UnityEngine.UI.Button>(); btn.onClick.Invoke(); return "Clicked";'

Write-Host "5. Waiting 6 seconds for timer to complete..."
Start-Sleep -Seconds 6

Write-Host "6. Getting Money after Timer..."
uloop execute-dynamic-code --code 'return SaveManager.Current.player.inventory.money.ToString();'

Write-Host "7. Stopping Play Mode..."
uloop control-play-mode --action stop
Start-Sleep -Seconds 4

Write-Host "8. Restarting Play Mode to test Save/Load..."
uloop control-play-mode --action play
Start-Sleep -Seconds 5

Write-Host "9. Getting Loaded Money..."
uloop execute-dynamic-code --code 'return SaveManager.Current.player.inventory.money.ToString();'

Write-Host "10. Stopping Play Mode..."
uloop control-play-mode --action stop
Start-Sleep -Seconds 3

Write-Host "11. Restoring Timer duration to 25m (1500s)..."
uloop execute-dynamic-code --code 'var tm = GameObject.Find("MainManager").GetComponent<TimerManager>(); tm.workDurationSeconds = 1500f; UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene()); return "Done";'

Write-Host "Test Completed Successfully!"
