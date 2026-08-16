/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using System.Collections.Generic;
using IRIS.Node;
using UnityEngine;

namespace IRIS.MetaQuest3.QRCodeDetection
{
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    public class QRCodeManager : Singleton<QRCodeManager>
    {
        public const string ScenePermission = OVRPermissionsRequester.ScenePermission;

        public static bool IsSupported
            => OVRAnchor.TrackerConfiguration.QRCodeTrackingSupported;

        private readonly Dictionary<string, MRUKTrackable> _trackedQRCodes = new();

        [SerializeField]
        private QRCode _qrCodePrefab;

        [SerializeField]
        private MRUK _mrukInstance;

        void OnEnable()
        {
            if (!_mrukInstance)
            {
                Debug.LogError($"{nameof(QRCodeManager)} requires an MRUK object in the scene!");
                return;
            }

            _mrukInstance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
            _mrukInstance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
        }

        void OnDisable()
        {
            if (!_mrukInstance)
            {
                return;
            }

            _mrukInstance.SceneSettings.TrackableAdded.RemoveListener(OnTrackableAdded);
            _mrukInstance.SceneSettings.TrackableRemoved.RemoveListener(OnTrackableRemoved);
            _trackedQRCodes.Clear();
        }

        public void OnTrackableAdded(MRUKTrackable trackable)
        {
            if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
            {
                return;
            }

            if (trackable.MarkerPayloadString == null)
            {
                return;
            }

            string normalizedPayload = NormalizePayload(trackable.MarkerPayloadString);

            _trackedQRCodes[normalizedPayload] = trackable;
            Debug.Log($"{nameof(OnTrackableAdded)}: QR code tracked. Payload: {normalizedPayload}");

            QRCode qrCode = Instantiate(_qrCodePrefab, trackable.transform);
            qrCode.Initialize(trackable);
            qrCode.GetComponent<Bounded2DVisualizer>().Initialize(trackable);
        }

        public void OnTrackableRemoved(MRUKTrackable trackable)
        {
            if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
            {
                return;
            }

            string normalizedPayload = NormalizePayload(trackable.MarkerPayloadString);

            if (normalizedPayload != null)
            {
                _trackedQRCodes.Remove(normalizedPayload);
            }

            Debug.Log($"{nameof(OnTrackableRemoved)}: {trackable.Anchor.Uuid.ToString("N").Remove(8)}[..]");
            Destroy(trackable.gameObject);
        }

        public bool TrackingEnabled
        {
            get => _mrukInstance && _mrukInstance.SceneSettings.TrackerConfiguration.QRCodeTrackingEnabled;
            set
            {
                if (!_mrukInstance)
                {
                    return;
                }

                var config = _mrukInstance.SceneSettings.TrackerConfiguration;
                config.QRCodeTrackingEnabled = value;
                _mrukInstance.SceneSettings.TrackerConfiguration = config;
            }
        }

        internal bool TryGetTrackedQRCode(string payload, out MRUKTrackable trackable)
            => _trackedQRCodes.TryGetValue(NormalizePayload(payload), out trackable);

        internal static string NormalizePayload(string payload)
            => payload?.TrimEnd('\0', '\r', '\n');

        public static bool HasPermissions
#if UNITY_EDITOR
            => true;
#else
            => UnityEngine.Android.Permission.HasUserAuthorizedPermission(ScenePermission);
#endif
    }
}
