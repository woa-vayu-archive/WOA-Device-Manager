﻿using FastBoot;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Helpers
{
    internal class FastBootProcedures
    {
        public static string GetProduct()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("product", out string productGetVar);
            return !result ? null : productGetVar;
        }

        public static void Reboot()
        {
            _ = DeviceManager.Device.FastBootTransport.Reboot();
        }

        public static void RebootBootloader()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootBootloader();
        }

        public static void RebootRecovery()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootRecovery();
        }

        public static void RebootFastBootD()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootFastBootD();
        }

        public static bool IsUnlocked()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("unlocked", out string unlockedVariable);
            if (!result)
            {
                return false;
            }

            return unlockedVariable == "yes";
        }

        public static bool CanUnlock()
        {
            bool result = DeviceManager.Device.FastBootTransport.FlashingGetUnlockAbility(out bool canUnlock);
            if (!result)
            {
                return false;
            }

            return canUnlock;
        }

        public static bool FlashUnlock(Control frameHost = null)
        {
            if (CanUnlock())
            {
                ContentDialog dialog = new()
                {
                    Title = "⚠️ EVERYTHING WILL BE FORMATTED",
                    Content = "Flash unlocking requires everything to be formatted. MAKE SURE YOU HAVE MADE A COPY OF EVERYTHING. We're not responsible for data loss.",
                    PrimaryButtonText = "⚠️ Proceed"
                };

                dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
                {
                    DeviceManager.Device.FastBootTransport.FlashingUnlock();
                    dialog.Hide();
                };

                dialog.CloseButtonText = "Cancel";

                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }
                _ = dialog.ShowAsync();

                return true;
            }
            else
            {
                ContentDialog dialog = new()
                {
                    Title = "Unlocking is disabled",
                    Content = "Flash Unlocking is disabled from Developer Settings in Android. Please enable it manually from there.",
                    CloseButtonText = "OK"
                };

                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }

                _ = dialog.ShowAsync();

                return false;
            }
        }

        public static bool FlashLock(Control frameHost = null)
        {
            // TODO: Check that the device doesn't have Windows installed
            ContentDialog dialog = new()
            {
                Title = "⚠️ Your bootloader will be locked",
                Content = "This procedure will lock your bootloader. You usually don't want to do this unless you have to sell your device.",
                PrimaryButtonText = "⚠️ Proceed"
            };

            dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
            {
                DeviceManager.Device.FastBootTransport.FlashingLock();
                dialog.Hide();
            };

            dialog.CloseButtonText = "Cancel";

            if (frameHost != null)
            {
                dialog.XamlRoot = frameHost.XamlRoot;
            }

            _ = dialog.ShowAsync();

            return true;
        }

        public static async Task<bool> BootTWRP()
        {
            if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
            {
                StorageFile twrp = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_EPSILON, true);
                return twrp != null && DeviceManager.Device.FastBootTransport.BootImageIntoRam(twrp.Path);
            }
            else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
            {
                StorageFile twrp = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_ZETA, true);
                return twrp != null && DeviceManager.Device.FastBootTransport.BootImageIntoRam(twrp.Path);
            }
            else
            {
                throw new Exception("Unknown device product");
            }
        }

        public static async Task<bool> BootUEFI()
        {
            if (DeviceManager.Device.Product == DeviceProduct.Epsilon)
            {
                StorageFile uefi = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.UEFI_EPSILON, true);
                if (uefi == null)
                {
                    return false;
                }

                string destinationPath = (await uefi.GetParentAsync()).Path;
                ZipFile.ExtractToDirectory(uefi.Path, destinationPath, true);

                return DeviceManager.Device.FastBootTransport.BootImageIntoRam($"{destinationPath}\\Surface Duo (1st Gen) UEFI (Fast Boot)\\uefi.img");
            }
            else if (DeviceManager.Device.Product == DeviceProduct.Zeta)
            {
                StorageFile uefi = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.UEFI_ZETA, true);
                if (uefi == null)
                {
                    return false;
                }

                string destinationPath = (await uefi.GetParentAsync()).Path;
                ZipFile.ExtractToDirectory(uefi.Path, destinationPath, true);

                return DeviceManager.Device.FastBootTransport.BootImageIntoRam($"{destinationPath}\\Surface Duo 2 UEFI (Fast Boot)\\uefi.img");
            }
            else
            {
                throw new Exception("Unknown device product");
            }
        }

        public static string GetDeviceBatteryLevel()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("battery-level", out string batteryLevel);
            return result ? batteryLevel : null;
        }
    }
}
