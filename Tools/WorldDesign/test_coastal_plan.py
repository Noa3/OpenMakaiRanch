import copy
import json
import math
import unittest

from coastal_plan import SOURCE, envelope, inside, intersects_plot, regional, ribbon, samples, validate


class CoastalPlanTests(unittest.TestCase):
    def setUp(self):
        self.data = json.loads(SOURCE.read_text(encoding='utf-8'))

    def test_source_plan_swept_paths_and_heights(self):
        self.assertEqual([], validate(self.data)['errors'])

    def test_stream_uphill_is_rejected(self):
        points = self.data['region']['stream']['points']
        points[3][1] = points[2][1] + 1
        self.assertIn('stream must descend throughout the sampled curve', validate(self.data)['errors'])

    def test_seam_mismatch_is_rejected(self):
        self.data['region']['areas']['town']['origin'][2] += 1
        self.assertTrue(any('scene seam gap' in error for error in validate(self.data)['errors']))

    def test_full_width_not_only_centerline(self):
        self.data['region']['connection']['width'] = 40
        self.assertTrue(any('VALLEY_LANE: swept path leaves' in error for error in validate(self.data)['errors']))

    def test_path_caps_are_included(self):
        points = list(ribbon([[0,0],[2,0]],2))
        self.assertAlmostEqual(-1,min(p[0] for p in points))
        self.assertAlmostEqual(3,max(p[0] for p in points))

    def test_rotated_envelope_is_not_its_bounding_box(self):
        plot = {'center':[0,0],'footprint':[2,6],'yaw':math.pi/4}
        x0,z0,x1,z1 = envelope(plot)
        self.assertTrue(x0 < 3 < x1 and z0 < 3 < z1)
        self.assertFalse(intersects_plot([3,3],plot))
        self.assertTrue(intersects_plot([0,0],plot))

    def test_scene_rotation_preserves_matching_regional_anchors(self):
        areas = self.data['region']['areas']
        ranch = regional(areas['ranch'],areas['ranch']['seam'])
        town = regional(areas['town'],areas['town']['seam'])
        self.assertLess(math.dist(ranch,town),1e-6)

    def test_smoothed_height_has_no_overshoot(self):
        points = samples([[0,10,0],[5,3,9],[11,0,12]])
        self.assertTrue(all(b[1] < a[1] for a,b in zip(points,points[1:])))
        self.assertEqual([11,0,12],points[-1])

    def test_polygon_boundary_is_inside_and_outside_rejected(self):
        polygon = [[0,0],[5,0],[5,5],[0,5]]
        self.assertTrue(inside([0,2],polygon))
        self.assertTrue(inside([2,2],polygon))
        self.assertFalse(inside([6,2],polygon))

    def test_conflicting_area_dimensions_rejected(self):
        self.data['ranch']['half_extents'] = [24.5,19.5]
        self.assertIn('ranch: conflicting area extents',validate(self.data)['errors'])


if __name__ == '__main__':
    unittest.main()
